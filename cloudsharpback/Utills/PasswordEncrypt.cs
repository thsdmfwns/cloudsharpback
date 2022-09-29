using BC = BCrypt.Net.BCrypt;

namespace cloudsharpback.Utills
{
    public static class Encrypt
    {
        public static string EncryptByBCrypt(string password) => BC.HashPassword(password);
        public static bool VerifyBCrypt(string text, string hash) => BC.Verify(text, hash);
    }
}
