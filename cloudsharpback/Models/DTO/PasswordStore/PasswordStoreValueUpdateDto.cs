namespace cloudsharpback.Models.DTO.PasswordStore;

public record PasswordStoreValueUpdateDto
{
    public string? ValueId { get; init; }
    public required string ValuePassword { get; init; }
}