namespace cloudsharpback.Models
{
    public class RegisterDto
    {
        public RegisterDto(string id, string pw, string email, string nick)
        {
            Id = id;
            Pw = pw;
            Email = email;
            Nick = nick;
        }

        public string Id { get; set; }
        public string Pw { get; set; }
        public string Email { get; set; }
        public string Nick { get; set; }
    }
}
