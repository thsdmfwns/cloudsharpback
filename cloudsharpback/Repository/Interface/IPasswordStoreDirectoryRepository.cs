using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.PasswordStore;

namespace cloudsharpback.Repository.Interface;

public interface IPasswordStoreDirectoryRepository
{
    public Task<List<PasswordStoreDirDto>> GetDirListByMemberId(ulong memberId);
    public Task<PasswordStoreDirDto?> GetDirById(ulong memberId, ulong dirId);
    public Task<bool> InsertDir(ulong memberId, string name, string? comment, string? icon);
    Task<bool> TryInsertDir(ulong memberId, string name, string? comment, string? icon);
    public Task<bool> InsertDir(ulong memberId, PasswordStoreDirInsertDto dto);
    public Task<bool> DeleteDir(ulong memberId, ulong id);
    public Task<bool> UpdateDir(ulong memberId, ulong itemId, string name, string? comment, string? icon);
    
}