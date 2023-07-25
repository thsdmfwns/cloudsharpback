using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces;

public interface IPasswordStoreService
{
    public Task<List<PasswordStoreDirDto>> GetDirList(MemberDto memberDto);
    public Task<HttpResponseDto?> MakeNewDir(MemberDto memberDto, PasswordStoreDirInsertDto dto);
}