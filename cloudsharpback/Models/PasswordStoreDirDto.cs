namespace cloudsharpback.Models;

public record PasswordStoreDirDto
{
    public required ulong Id { get; set; }
    public required string Name { get; set; }
    public string? Comment { get; set; }
    public string? Icon { get; set; }
    public required ulong LastEditTime { get; set; }
    public required ulong CreatedTime { get; set; }
    public required ulong OwnerId { get; set; }
}