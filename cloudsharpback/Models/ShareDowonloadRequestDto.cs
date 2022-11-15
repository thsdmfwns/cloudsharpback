namespace cloudsharpback.Models
{
    public class ShareDowonloadRequestDto
    {
        public ShareDowonloadRequestDto(string token, string? password)
        {
            Token = token;
            Password = password;
        }

        public string Token { get; set; }
        public string? Password { get; set; }
    }
}
