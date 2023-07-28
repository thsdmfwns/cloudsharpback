namespace cloudsharpback.Models.DTO.PasswordStore;

public record PasswordStoreKeyDto
{
    public required ulong Id { get; init; }
    public required ulong OwnerId { get; init; }
    public string? PublicKey { get; init; }
    public required string PrivateKey { get; init; }
    public required string Name { get; init; }
    public string? Comment { get; init; }
    public required ulong CreatedTime { get; init; }
    public required ulong EncryptAlgorithmValue { get; init; }
    public PasswordEncryptAlgorithm EncryptAlgorithm => (PasswordEncryptAlgorithm)EncryptAlgorithmValue;
}