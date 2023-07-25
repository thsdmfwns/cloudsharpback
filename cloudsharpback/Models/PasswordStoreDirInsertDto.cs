namespace cloudsharpback.Models;

public record PasswordStoreDirInsertDto
{
    public required string Name { get; set; }
    public string? Icon { get; set; }
    public string? Comment { get; set; }
}