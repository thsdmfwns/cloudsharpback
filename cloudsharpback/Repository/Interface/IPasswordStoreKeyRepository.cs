using cloudsharpback.Models;

namespace cloudsharpback.Repository.Interface;

public interface IPasswordStoreKeyRepository
{
    public Task<PasswordStoreKeyDto?> GetKeyById(ulong keyId);
}