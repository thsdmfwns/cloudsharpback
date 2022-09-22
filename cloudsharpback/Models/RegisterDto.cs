namespace cloudsharpback.Models
{
    public class RegisterDto
    {
        public RegisterDto(LoginDto login, int role, string email, string nick)
        {
            Login = login;
            Role = role;
            Email = email;
            Nickname = nick;
        }

        public LoginDto Login { get; set; }
        public int Role { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
    }
}
