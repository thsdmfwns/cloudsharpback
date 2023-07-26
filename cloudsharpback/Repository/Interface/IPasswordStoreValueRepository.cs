using cloudsharpback.Models;

namespace cloudsharpback.Repository.Interface;

public interface IPasswordStoreValueRepository
{
    Task<List<PasswordStoreValueDto>> GetPasswordStoreValuesByDirectoryId(ulong dirId);
    Task<List<PasswordStoreValueDto>> GetPasswordStoreValuesByKeyId(ulong keyId);
    Task<List<PasswordStoreValueDto>> GetPasswordStoreValuesByKeyIdAndDirId(ulong dirId ,ulong keyId);
    Task<bool> InsertValue(ulong dirId, ulong keyId, string? valueId, string valuePassword);
}