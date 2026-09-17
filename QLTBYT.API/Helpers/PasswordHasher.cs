namespace QLTBYT.API.Helpers;

public static class PasswordHasher
{
    public static string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public static bool Verify(string password, string storedHashOrPlain)
    {
        if (string.IsNullOrEmpty(storedHashOrPlain))
        {
            return false;
        }

        if (storedHashOrPlain.StartsWith("$2", StringComparison.Ordinal))
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHashOrPlain);
        }

        return storedHashOrPlain == password;
    }

    public static bool IsBcryptHash(string? value) =>
        !string.IsNullOrEmpty(value) && value.StartsWith("$2", StringComparison.Ordinal);
}
