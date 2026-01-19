using Identity.Core.Entities;
using Identity.Core.Interfaces;
using Shared.Domain.Results;
using Shared.Messaging.Abstractions;
using Shared.Contracts.Events;

namespace Identity.Core.Services;

/// <summary>
/// Service for authentication operations.
/// </summary>
public sealed class AuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMfaService _mfaService;
    private readonly IEventPublisher _eventPublisher;
    private readonly AuthenticationOptions _options;

    public AuthenticationService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IMfaService mfaService,
        IEventPublisher eventPublisher,
        AuthenticationOptions options)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mfaService = mfaService;
        _eventPublisher = eventPublisher;
        _options = options;
    }

    public async Task<Result<AuthenticationResult>> RegisterAsync(
        string email,
        string password,
        string? displayName = null,
        CancellationToken cancellationToken = default)
    {
        // Check if user exists
        if (await _unitOfWork.Users.ExistsByEmailAsync(email, cancellationToken))
        {
            return Error.Conflict("A user with this email already exists");
        }

        // Validate password strength
        var passwordValidation = ValidatePassword(password);
        if (passwordValidation.IsFailure)
        {
            return passwordValidation.Error;
        }

        // Create user
        var passwordHash = _passwordHasher.Hash(password);
        var user = User.Create(email, passwordHash, displayName);

        // Assign default role
        var userRole = await _unitOfWork.Roles.GetByNameAsync(Role.SystemRoles.User, cancellationToken);
        if (userRole != null)
        {
            user.AddRole(userRole);
        }

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var token = RefreshToken.Create(user.Id, refreshToken, _tokenService.RefreshTokenLifetime);

        // Add user and refresh token to context
        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.AddRefreshTokenAsync(token, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish event after successful save
        await _eventPublisher.PublishAsync(new UserCreated
        {
            UserIdentifier = user.Id.ToString(),
            Email = user.Email,
            DisplayName = user.DisplayName,
            CreatedBy = "system"
        }, cancellationToken);

        return new AuthenticationResult
        {
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = (int)_tokenService.AccessTokenLifetime.TotalSeconds
        };
    }

    public async Task<Result<AuthenticationResult>> LoginAsync(
        string email,
        string password,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (user == null)
        {
            return Error.Unauthorized("Invalid email or password");
        }

        if (!user.IsActive)
        {
            return Error.Unauthorized("Account is deactivated");
        }

        if (user.IsLockedOut)
        {
            return Error.Unauthorized("Account is locked due to too many failed login attempts");
        }

        if (!_passwordHasher.Verify(password, user.PasswordHash))
        {
            user.RecordFailedLogin(_options.MaxFailedAttempts, _options.LockoutDuration);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Error.Unauthorized("Invalid email or password");
        }

        // Check MFA
        if (user.IsMfaEnabled)
        {
            return new AuthenticationResult
            {
                UserId = user.Id,
                Email = user.Email,
                RequiresMfa = true
            };
        }

        return await CompleteLoginAsync(user, ipAddress, cancellationToken);
    }

    public async Task<Result<AuthenticationResult>> VerifyMfaAsync(
        Guid userId,
        string code,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Error.NotFound("User", userId.ToString());
        }

        if (!user.IsMfaEnabled || string.IsNullOrEmpty(user.MfaSecret))
        {
            return Error.Validation("MFA is not enabled for this user");
        }

        if (!_mfaService.VerifyCode(user.MfaSecret, code))
        {
            return Error.Unauthorized("Invalid MFA code");
        }

        return await CompleteLoginAsync(user, ipAddress, cancellationToken);
    }

    public async Task<Result<AuthenticationResult>> RefreshTokenAsync(
        string refreshToken,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByRefreshTokenAsync(refreshToken, cancellationToken);
        if (user == null)
        {
            return Error.Unauthorized("Invalid refresh token");
        }

        var token = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
        if (token == null || !token.IsActive)
        {
            return Error.Unauthorized("Invalid or expired refresh token");
        }

        // Rotate refresh token
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        token.Revoke(ipAddress, newRefreshToken);

        var newToken = RefreshToken.Create(user.Id, newRefreshToken, _tokenService.RefreshTokenLifetime, ipAddress);
        await _unitOfWork.AddRefreshTokenAsync(newToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync(new TokenRefreshed
        {
            UserIdentifier = user.Id.ToString(),
            NewExpiration = DateTimeOffset.UtcNow.Add(_tokenService.AccessTokenLifetime),
            IpAddress = ipAddress
        }, cancellationToken);

        return new AuthenticationResult
        {
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AccessToken = _tokenService.GenerateAccessToken(user),
            RefreshToken = newRefreshToken,
            ExpiresIn = (int)_tokenService.AccessTokenLifetime.TotalSeconds
        };
    }

    public async Task<Result> LogoutAsync(
        Guid userId,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Error.NotFound("User", userId.ToString());
        }

        user.RevokeRefreshToken(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new UserLoggedOut
        {
            UserIdentifier = userId.ToString(),
            LogoutReason = "User logout"
        }, cancellationToken);

        return Result.Success();
    }

    private async Task<AuthenticationResult> CompleteLoginAsync(
        User user,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        user.RecordLogin();

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var token = RefreshToken.Create(user.Id, refreshToken, _tokenService.RefreshTokenLifetime, ipAddress);

        // Add refresh token directly to context to avoid EF Core tracking issues
        await _unitOfWork.AddRefreshTokenAsync(token, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new UserLoggedIn
        {
            UserIdentifier = user.Id.ToString(),
            Email = user.Email,
            LoginMethod = "password",
            IpAddress = ipAddress
        }, cancellationToken);

        return new AuthenticationResult
        {
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = (int)_tokenService.AccessTokenLifetime.TotalSeconds
        };
    }

    public async Task<Result<EmailVerificationResult>> RequestEmailVerificationAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Error.NotFound("User", userId.ToString());
        }

        if (user.IsEmailVerified)
        {
            return Error.Validation("Email is already verified");
        }

        var token = _tokenService.GenerateEmailVerificationToken(user.Id);

        return new EmailVerificationResult
        {
            UserId = user.Id,
            Email = user.Email,
            Token = token
        };
    }

    public async Task<Result> VerifyEmailAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var userId = _tokenService.ValidateEmailVerificationToken(token);
        if (userId == null)
        {
            return Error.Unauthorized("Invalid or expired email verification token");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            return Error.NotFound("User", userId.Value.ToString());
        }

        if (user.IsEmailVerified)
        {
            return Error.Validation("Email is already verified");
        }

        user.VerifyEmail();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new UserEmailVerified
        {
            UserIdentifier = user.Id.ToString(),
            Email = user.Email
        }, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<PasswordResetResult>> ForgotPasswordAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (user == null)
        {
            // Return success to prevent email enumeration
            return new PasswordResetResult { Email = email };
        }

        if (!user.IsActive)
        {
            // Return success to prevent email enumeration
            return new PasswordResetResult { Email = email };
        }

        var token = _tokenService.GeneratePasswordResetToken(user.Id);

        return new PasswordResetResult
        {
            UserId = user.Id,
            Email = user.Email,
            Token = token
        };
    }

    public async Task<Result> ResetPasswordAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var userId = _tokenService.ValidatePasswordResetToken(token);
        if (userId == null)
        {
            return Error.Unauthorized("Invalid or expired password reset token");
        }

        var passwordValidation = ValidatePassword(newPassword);
        if (passwordValidation.IsFailure)
        {
            return passwordValidation.Error;
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            return Error.NotFound("User", userId.Value.ToString());
        }

        var passwordHash = _passwordHasher.Hash(newPassword);
        user.UpdatePassword(passwordHash);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new UserPasswordChanged
        {
            UserIdentifier = user.Id.ToString(),
            Email = user.Email
        }, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<MfaSetupResult>> GetMfaSetupAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Error.NotFound("User", userId.ToString());
        }

        if (user.IsMfaEnabled)
        {
            return Error.Validation("MFA is already enabled for this account");
        }

        var secret = _mfaService.GenerateSecret();
        var qrCodeUri = _mfaService.GenerateQrCodeUri(user.Email, secret);

        return new MfaSetupResult
        {
            Secret = secret,
            QrCodeUri = qrCodeUri
        };
    }

    public async Task<Result<MfaEnabledResult>> EnableMfaAsync(
        Guid userId,
        string secret,
        string verificationCode,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Error.NotFound("User", userId.ToString());
        }

        if (user.IsMfaEnabled)
        {
            return Error.Validation("MFA is already enabled for this account");
        }

        // Verify the code before enabling
        if (!_mfaService.VerifyCode(secret, verificationCode))
        {
            return Error.Unauthorized("Invalid verification code");
        }

        var backupCodes = _mfaService.GenerateBackupCodes();
        user.EnableMfa(secret);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new MfaEnabled
        {
            UserIdentifier = user.Id.ToString(),
            Email = user.Email
        }, cancellationToken);

        return new MfaEnabledResult
        {
            BackupCodes = backupCodes
        };
    }

    public async Task<Result> DisableMfaAsync(
        Guid userId,
        string code,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Error.NotFound("User", userId.ToString());
        }

        if (!user.IsMfaEnabled || string.IsNullOrEmpty(user.MfaSecret))
        {
            return Error.Validation("MFA is not enabled for this account");
        }

        if (!_mfaService.VerifyCode(user.MfaSecret, code))
        {
            return Error.Unauthorized("Invalid MFA code");
        }

        user.DisableMfa();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new MfaDisabled
        {
            UserIdentifier = user.Id.ToString(),
            Email = user.Email
        }, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Error.NotFound("User", userId.ToString());
        }

        if (!_passwordHasher.Verify(currentPassword, user.PasswordHash))
        {
            return Error.Unauthorized("Current password is incorrect");
        }

        var passwordValidation = ValidatePassword(newPassword);
        if (passwordValidation.IsFailure)
        {
            return passwordValidation.Error;
        }

        var passwordHash = _passwordHasher.Hash(newPassword);
        user.UpdatePassword(passwordHash);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new UserPasswordChanged
        {
            UserIdentifier = user.Id.ToString(),
            Email = user.Email
        }, cancellationToken);

        return Result.Success();
    }

    private Result ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return Error.Validation("Password is required");
        }

        if (password.Length < _options.MinPasswordLength)
        {
            return Error.Validation($"Password must be at least {_options.MinPasswordLength} characters");
        }

        if (_options.RequireUppercase && !password.Any(char.IsUpper))
        {
            return Error.Validation("Password must contain at least one uppercase letter");
        }

        if (_options.RequireLowercase && !password.Any(char.IsLower))
        {
            return Error.Validation("Password must contain at least one lowercase letter");
        }

        if (_options.RequireDigit && !password.Any(char.IsDigit))
        {
            return Error.Validation("Password must contain at least one digit");
        }

        if (_options.RequireSpecialChar && !password.Any(c => !char.IsLetterOrDigit(c)))
        {
            return Error.Validation("Password must contain at least one special character");
        }

        return Result.Success();
    }
}

/// <summary>
/// Result of an authentication operation.
/// </summary>
public sealed class AuthenticationResult
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public int ExpiresIn { get; init; }
    public bool RequiresMfa { get; init; }
}

/// <summary>
/// Configuration options for authentication.
/// </summary>
public sealed class AuthenticationOptions
{
    public int MinPasswordLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialChar { get; set; } = false;
    public int MaxFailedAttempts { get; set; } = 5;
    public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);
}

/// <summary>
/// Result of an email verification request.
/// </summary>
public sealed class EmailVerificationResult
{
    public Guid? UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Token { get; init; }
}

/// <summary>
/// Result of a password reset request.
/// </summary>
public sealed class PasswordResetResult
{
    public Guid? UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Token { get; init; }
}

/// <summary>
/// Result of MFA setup request.
/// </summary>
public sealed class MfaSetupResult
{
    public string Secret { get; init; } = string.Empty;
    public string QrCodeUri { get; init; } = string.Empty;
}

/// <summary>
/// Result of enabling MFA.
/// </summary>
public sealed class MfaEnabledResult
{
    public IReadOnlyList<string> BackupCodes { get; init; } = Array.Empty<string>();
}
