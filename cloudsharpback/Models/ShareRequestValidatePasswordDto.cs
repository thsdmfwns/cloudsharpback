namespace cloudsharpback.Models;

public class ShareRequestValidatePasswordDto
{
    public required string Token { get; set; }
    public required string Password { get; set; }
}