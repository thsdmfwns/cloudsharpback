namespace cloudsharpback.Models;

public record PasswordStoreValueDto
{
    public required ulong Id { get; init; }
    public required ulong DirectoryId { get; init; }
    public required ulong KeyId { get; init; }
    public required ulong CreatedTime { get; init; }
    public required ulong LastEditedTime { get; init; }
    public string? ValueId { get; init; }
    public required string ValuePassword { get; init; }
}