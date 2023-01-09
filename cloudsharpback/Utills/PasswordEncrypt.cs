using BC = BCrypt.Net.BCrypt;

namespace cloudsharpback.Utills
{
    public static class PasswordEncrypt
    {
        public static string EncryptPassword(string password) => Base64.Encode(BC.HashPassword(password));
        public static bool VerifyPassword(string text, string hash) => BC.Verify(text, Base64.Decode(hash));
    }
}
