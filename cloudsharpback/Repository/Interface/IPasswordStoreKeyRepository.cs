using cloudsharpback.Models;

namespace cloudsharpback.Repository.Interface;

public interface IPasswordStoreKeyRepository
{
    public Task<PasswordStoreKeyDto?> GetKeyById(ulong keyId);
    public Task<List<PasswordStoreKeyDto>> GetKeyListByMemberId(ulong memberId);
    public Task<bool> InsertKey(ulong memberId, int encryptAlgorithm, string? publicKey, string privateKey);
    public Task<bool> DeleteKeyById(ulong memberId, ulong itemId);
}