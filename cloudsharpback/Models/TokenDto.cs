namespace cloudsharpback.Models
{
    public class TokenDto
    {
        public TokenDto(string acessToken, string refreshToken)
        {
            AcessToken = acessToken;
            RefreshToken = refreshToken;
        }

        public string AcessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
