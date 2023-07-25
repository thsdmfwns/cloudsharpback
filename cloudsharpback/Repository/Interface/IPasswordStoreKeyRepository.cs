using cloudsharpback.Models;

namespace cloudsharpback.Repository.Interface;

public interface IPasswordStoreKeyRepository
{
    public Task<PasswordStoreKeyDto?> GetKeyById(ulong keyId);
    public Task<bool> InsertKey(ulong memberId, ulong encryptAlgorithm, string publicKey, string privateKey);
}