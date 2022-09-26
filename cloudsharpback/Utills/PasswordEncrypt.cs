using BC = BCrypt.Net.BCrypt;

namespace cloudsharpback.Utills
{
    public static class PasswordEncrypt
    {
        public static string EncryptPassword(string password) => BC.HashPassword(password);
        public static bool VerifyPassword(string password, string? passwordHash)
        {
            if (passwordHash is null) return false;
            return BC.Verify(password, passwordHash);
        }
    }
}
