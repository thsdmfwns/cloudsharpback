using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.PasswordStore;

namespace cloudsharpback.Repository.Interface;

public interface IPasswordStoreKeyRepository
{
    public Task<PasswordStoreKeyDto?> GetKeyById(ulong memberId, ulong keyId);
    public Task<List<PasswordStoreKeyListItemDto>> GetKeyListByMemberId(ulong memberId);
    public Task<bool> TryInsertKey(ulong memberId, int encryptAlgorithm, string? publicKey, string privateKey, string name,
        string? comment);
    public Task<bool> DeleteKeyById(ulong memberId, ulong itemId);
}