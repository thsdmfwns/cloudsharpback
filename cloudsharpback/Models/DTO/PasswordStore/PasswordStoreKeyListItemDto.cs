namespace cloudsharpback.Models.DTO.PasswordStore;

public class PasswordStoreKeyListItemDto
{
    public required ulong Id { get; init; }
    public required ulong OwnerId { get; init; }
    public required ulong EncryptAlgorithmValue { get; init; }
    public required string Name { get; init; }
    public string? Comment { get; init; }
    public required ulong CreatedTime { get; init; }
    public PasswordEncryptAlgorithm EncryptAlgorithm => (PasswordEncryptAlgorithm)EncryptAlgorithmValue;
}