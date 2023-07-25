namespace cloudsharpback.Models;

public record PasswordStoreDirInsertDto
{
    public required string Name { get; init; }
    public string? Icon { get; init; }
    public string? Comment { get; init; }
}