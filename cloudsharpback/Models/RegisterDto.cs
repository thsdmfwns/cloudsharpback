namespace cloudsharpback.Models
{
    public class RegisterDto
    {
        public LoginDto? Login { get; set; }
        public ulong Role { get; set; }
        public string? Email { get; set; }
        public string? Nickname { get; set; }
    }
}
