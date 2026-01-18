using System.Security.Cryptography;
using Identity.Core.Interfaces;
using OtpNet;

namespace Identity.Infrastructure.Services;

/// <summary>
/// TOTP-based Multi-Factor Authentication service.
/// </summary>
internal sealed class TotpMfaService : IMfaService
{
    private const int SecretSize = 20;
    private const int BackupCodeLength = 8;

    public string GenerateSecret()
    {
        var key = KeyGeneration.GenerateRandomKey(SecretSize);
        return Base32Encoding.ToString(key);
    }

    public string GenerateCode(string secret)
    {
        var key = Base32Encoding.ToBytes(secret);
        var totp = new Totp(key);
        return totp.ComputeTotp();
    }

    public bool VerifyCode(string secret, string code)
    {
        var key = Base32Encoding.ToBytes(secret);
        var totp = new Totp(key);
        return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
    }

    public string GenerateQrCodeUri(string email, string secret, string issuer = "NGMAT")
    {
        // Generate otpauth URI for authenticator apps
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail = Uri.EscapeDataString(email);
        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secret}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
    }

    public IReadOnlyList<string> GenerateBackupCodes(int count = 10)
    {
        var codes = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            codes.Add(GenerateBackupCode());
        }
        return codes;
    }

    private static string GenerateBackupCode()
    {
        var bytes = new byte[BackupCodeLength / 2];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }
}
