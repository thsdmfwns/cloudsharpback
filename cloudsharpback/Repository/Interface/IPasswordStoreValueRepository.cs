using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.PasswordStore;

namespace cloudsharpback.Repository.Interface;

public interface IPasswordStoreValueRepository
{
    Task<PasswordStoreValueDto?> GetValueById(ulong id);
    Task<List<PasswordStoreValueListItemDto>> GetValuesByDirectoryId(ulong dirId);
    Task<List<PasswordStoreValueListItemDto>> GetValuesByKeyId(ulong keyId);
    Task<List<PasswordStoreValueListItemDto>> GetValuesByKeyIdAndDirId(ulong dirId ,ulong keyId);
    Task<bool> InsertValue(ulong dirId, ulong keyId, string? valueId, string valuePassword);
    Task<bool> UpdateValue(ulong itemId, string? valueId, string valuePassword);
    Task<bool> DeleteValue(ulong itemId);
    
}