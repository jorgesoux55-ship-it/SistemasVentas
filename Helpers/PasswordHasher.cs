using System.Security.Cryptography;
using System.Text;

namespace SistemaVentas.Helpers;

public static class PasswordHasher
{
    // Sal fija para hashing consistente en la aplicación
    private const string Salt = "SistemaVentas_SecureSalt_2026_#XyZ!";

    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return string.Empty;
        
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password + Salt);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        var computed = HashPassword(password);
        return computed == storedHash;
    }
}
