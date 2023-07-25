using cloudsharpback.Models;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;

namespace cloudsharpback.Services;

public class PasswordStoreService : IPasswordStoreService
{
    private readonly IPasswordStoreDirectoryRepository _directoryRepository;

    public PasswordStoreService(IPasswordStoreDirectoryRepository directoryRepository)
    {
        _directoryRepository = directoryRepository;
    }


    public async Task<List<PasswordStoreDirDto>> GetDirList(MemberDto memberDto)
    {
        return await _directoryRepository.GetDirListByMemberId(memberDto.Id);
    }

    public async Task<HttpResponseDto?> MakeNewDir(MemberDto memberDto, PasswordStoreDirInsertDto dto)
    {
        if (!(await _directoryRepository.InstertDir(memberDto.Id, dto.Name, dto.Comment, dto.Icon)))
        {
            return new HttpResponseDto() { HttpCode = 400 };
        }
        return null;
    }
}