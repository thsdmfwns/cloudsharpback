using System.Text;

namespace cloudsharpback.Utils
{
    public static class Base64
    {
        public static string Encode(string text)
            => Convert.ToBase64String(Encoding.Unicode.GetBytes(text));
        public static string Decode(string base64)
            => Encoding.Unicode.GetString(Convert.FromBase64String(base64));
    }
}
