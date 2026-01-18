namespace Identity.Core.Interfaces;

/// <summary>
/// Interface for Multi-Factor Authentication operations.
/// </summary>
public interface IMfaService
{
    /// <summary>
    /// Generates a new MFA secret.
    /// </summary>
    string GenerateSecret();

    /// <summary>
    /// Generates a TOTP code for the given secret.
    /// </summary>
    string GenerateCode(string secret);

    /// <summary>
    /// Verifies a TOTP code against a secret.
    /// </summary>
    bool VerifyCode(string secret, string code);

    /// <summary>
    /// Generates a QR code URI for authenticator apps.
    /// </summary>
    string GenerateQrCodeUri(string email, string secret, string issuer = "NGMAT");

    /// <summary>
    /// Generates backup codes for account recovery.
    /// </summary>
    IReadOnlyList<string> GenerateBackupCodes(int count = 10);
}
