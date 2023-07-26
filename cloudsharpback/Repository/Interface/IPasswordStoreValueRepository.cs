using cloudsharpback.Models;

namespace cloudsharpback.Repository.Interface;

public interface IPasswordStoreValueRepository
{
    Task<PasswordStoreValueDto?> GetValueById(ulong id);
    Task<List<PasswordStoreValueDto>> GetValuesByDirectoryId(ulong dirId);
    Task<List<PasswordStoreValueDto>> GetValuesByKeyId(ulong keyId);
    Task<List<PasswordStoreValueDto>> GetValuesByKeyIdAndDirId(ulong dirId ,ulong keyId);
    Task<bool> InsertValue(ulong dirId, ulong keyId, string? valueId, string valuePassword);
    Task<bool> UpdateValue(ulong itemId, string? valueId, string valuePassword);
    Task<bool> DeleteValue(ulong itemId);
    
}