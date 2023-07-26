namespace cloudsharpback.Models;

public record PasswordStoreKeyInsertDto
{
    public required int EncryptAlgorithm { get; init; }   
    public string? PublicKey { get; init; }   
    public required string PrivateKey { get; init; }   
}