namespace cloudsharpback.Models.DTO.PasswordStore;

public record PasswordStoreValueInsertDto
{
    public required ulong DirectoryId { get; init; }     
    public required ulong KeyId { get; init; }     
    public string? ValueId { get; init; }     
    public required string ValuePassword { get; init; }     
}