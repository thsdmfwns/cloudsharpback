namespace cloudsharpback.Models;

public record PasswordStoreDirDto
{
    public required ulong Id;
    public required string Name;
    public string? Comment;
    public string? Icon;
    public required ulong LastEditTime;
    public required ulong CreatedTime;
    public required ulong OwnerId;
}