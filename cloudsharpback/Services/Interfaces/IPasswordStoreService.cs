using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces;

public interface IPasswordStoreService
{
    public Task<List<PasswordStoreDirDto>> GetDirList(MemberDto memberDto);
    public Task<HttpResponseDto?> MakeNewDir(MemberDto memberDto, PasswordStoreDirInsertDto dto);
    public Task<HttpResponseDto?> RemoveDir(MemberDto memberDto, ulong id);
    public Task<HttpResponseDto?> UpdateDir(MemberDto memberDto, PasswordStoreDirInsertDto dto, ulong itemId);
    public Task<(PasswordStoreValueDto? value, HttpResponseDto? err)> GetValue(MemberDto memberDto, ulong itemId);
    public Task<(List<PasswordStoreValueListItemDto> value, HttpResponseDto? err)> GetValuesList(MemberDto memberDto,
        ulong? keyId,
        ulong? dirId);
    public Task<HttpResponseDto?> MakeNewValue(MemberDto memberDto, PasswordStoreValueInsertDto dto);
    public Task<HttpResponseDto?> RemoveValue(MemberDto memberDto, ulong itemId);
    public Task<HttpResponseDto?> UpdateValue(MemberDto memberDto, ulong itemId, PasswordStoreValueUpdateDto dto);
    public Task<List<PasswordStoreKeyListItemDto>> GetKeyList(MemberDto memberDto);
    public Task<HttpResponseDto?> MakeNewKey(MemberDto memberDto, PasswordStoreKeyInsertDto dto);
    public Task<HttpResponseDto?> RemoveKey(MemberDto memberDto, ulong itemId);
}