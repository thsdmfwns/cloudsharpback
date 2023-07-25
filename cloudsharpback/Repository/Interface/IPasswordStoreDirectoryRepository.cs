using cloudsharpback.Models;

namespace cloudsharpback.Repository.Interface;

public interface IPasswordStoreDirectoryRepository
{
    public Task<List<PasswordStoreDirDto>> GetDirListByMemberId(ulong memberId);
    public Task<bool> InstertDir(ulong memberId, string name, string? comment, string? icon);
    public Task<bool> DeleteDir(ulong memberId, ulong id);
    public Task<bool> UpdateDir(ulong memberId, ulong itemId, string name, string? comment, string? icon);
    
}