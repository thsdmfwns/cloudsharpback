using cloudsharpback.Models;

namespace cloudsharpback.Repository.Interface;

public interface IPasswordStoreDirectoryRepository
{
    public Task<List<PasswordStoreDirDto>> GetDirListByMemberId(ulong memberId);
    public Task<bool> InstertDir(ulong memberId, string name, string? comment, string? icon);
    
}