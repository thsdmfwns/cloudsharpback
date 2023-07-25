namespace cloudsharpback.Models;

public record PasswordStoreKeyDto
{
    public required ulong Id { get; init; }
    public required ulong OwnerId { get; init; }
    public string? PublicKey { get; init; }
    public required string PrivateKey { get; init; }
    public required ulong EncryptAlgorithmValue { get; init; }
    public PasswordEncryptAlgorithm EncryptAlgorithm => (PasswordEncryptAlgorithm)EncryptAlgorithmValue;
}