namespace cloudsharpback.Models;

public record PasswordStoreKeyInsertDto
{
    public required ulong OwnerId { get; init; }   
    public required ulong EncryptAlgorithm { get; init; }   
    public string? PublicKey { get; init; }   
    public required string PrivateKey { get; init; }   
}