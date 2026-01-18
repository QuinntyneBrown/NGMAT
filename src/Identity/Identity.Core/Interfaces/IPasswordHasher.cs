namespace Identity.Core.Interfaces;

/// <summary>
/// Interface for password hashing operations.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>The hashed password.</returns>
    string Hash(string password);

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <param name="hash">The password hash.</param>
    /// <returns>True if the password matches the hash.</returns>
    bool Verify(string password, string hash);
}
