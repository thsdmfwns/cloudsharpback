using BC = BCrypt.Net.BCrypt;

namespace cloudsharpback.Utills
{
    public static class PasswordEncrypt
    {
        public static string EncryptPassword(string password) => BC.HashPassword(password);
    }
}
