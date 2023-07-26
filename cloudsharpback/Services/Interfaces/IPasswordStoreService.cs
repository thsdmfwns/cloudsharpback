using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces;

public interface IPasswordStoreService
{
    public Task<List<PasswordStoreDirDto>> GetDirList(MemberDto memberDto);
    public Task<HttpResponseDto?> MakeNewDir(MemberDto memberDto, PasswordStoreDirInsertDto dto);
    public Task<HttpResponseDto?> RemoveDir(MemberDto memberDto, ulong id);
    public Task<HttpResponseDto?> UpdateDir(MemberDto memberDto, PasswordStoreDirInsertDto dto, ulong itemId);
    public Task<(List<PasswordStoreValueDto> value, HttpResponseDto? err)> GetValuesList(MemberDto memberDto,
        ulong? keyId,
        ulong? dirId);
    public Task<HttpResponseDto?> MakeNewKey(MemberDto memberDto, PasswordStoreKeyInsertDto dto);
}