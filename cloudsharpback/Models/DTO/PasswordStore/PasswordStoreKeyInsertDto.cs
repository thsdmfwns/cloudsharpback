namespace cloudsharpback.Models.DTO.PasswordStore;

public record PasswordStoreKeyInsertDto
{
    public required int EncryptAlgorithm { get; init; }   
    public string? PublicKey { get; init; }   
    public required string PrivateKey { get; init; }   
    public required string Name { get; init; }   
    public string? Comment { get; init; }   
    
}