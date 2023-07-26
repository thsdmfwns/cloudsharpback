using cloudsharpback.Models;

namespace cloudsharpback.Repository.Interface;

public interface IPasswordStoreKeyRepository
{
    public Task<PasswordStoreKeyDto?> GetKeyById(ulong memberId, ulong keyId);
    public Task<List<PasswordStoreKeyListItemDto>> GetKeyListByMemberId(ulong memberId);
    public Task<bool> InsertKey(ulong memberId, int encryptAlgorithm, string? publicKey, string privateKey, string name,
        string? comment);
    public Task<bool> DeleteKeyById(ulong memberId, ulong itemId);
}