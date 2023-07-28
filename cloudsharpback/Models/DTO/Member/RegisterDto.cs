namespace cloudsharpback.Models.DTO.Member
{
    public class RegisterDto
    {
        public required string Id { get; set; }
        public required string Pw { get; set; }
        public required string Email { get; set; }
        public required string Nick { get; set; }
    }
}
