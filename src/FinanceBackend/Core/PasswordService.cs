namespace FinanceBackend.Core;

public static class PasswordService
{
    public static string Hash(string plainText) =>
        BCrypt.Net.BCrypt.HashPassword(plainText, workFactor: 12);

    public static bool Verify(string plainText, string hash) =>
        BCrypt.Net.BCrypt.Verify(plainText, hash);
}
