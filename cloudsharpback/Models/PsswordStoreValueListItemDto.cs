namespace cloudsharpback.Models;

public record PasswordStoreValueListItemDto
{
    public required ulong Id { get; init; }
    public required ulong DirectoryId { get; init; }
    public required ulong KeyId { get; init; }
    public required ulong CreatedTime { get; init; }
    public required ulong LastEditedTime { get; init; }
}