using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Models.DTO.PasswordStore;

namespace cloudsharpback.Services.Interfaces;

public interface IPasswordStoreService
{
    public Task<List<PasswordStoreDirDto>> GetDirList(MemberDto memberDto);
    /// <returns>400</returns>
    public Task<HttpResponseDto?> MakeNewDir(MemberDto memberDto, PasswordStoreDirInsertDto dto);
    /// <returns>404</returns>
    public Task<HttpResponseDto?> RemoveDir(MemberDto memberDto, ulong id);
    /// <returns>404</returns>
    public Task<HttpResponseDto?> UpdateDir(MemberDto memberDto, PasswordStoreDirInsertDto dto, ulong itemId);
    /// <returns>404, 403</returns>
    public Task<(PasswordStoreValueDto? value, HttpResponseDto? err)> GetValue(MemberDto memberDto, ulong itemId);
    /// <returns>403</returns>
    public Task<(List<PasswordStoreValueListItemDto> value, HttpResponseDto? err)> GetValuesList(MemberDto memberDto,
        ulong? keyId,
        ulong? dirId);
    /// <returns>403, 400</returns>
    public Task<HttpResponseDto?> MakeNewValue(MemberDto memberDto, PasswordStoreValueInsertDto dto);
    /// <returns>403, 404</returns>
    public Task<HttpResponseDto?> RemoveValue(MemberDto memberDto, ulong itemId);
    /// <returns>403, 404</returns>
    public Task<HttpResponseDto?> UpdateValue(MemberDto memberDto, ulong itemId, PasswordStoreValueUpdateDto dto);
    /// <returns>404</returns>
    public Task<(PasswordStoreKeyDto? value, HttpResponseDto? err)> GetKey(MemberDto memberDto, ulong itemId);
    public Task<List<PasswordStoreKeyListItemDto>> GetKeyList(MemberDto memberDto);
    /// <returns>400</returns>
    public Task<HttpResponseDto?> MakeNewKey(MemberDto memberDto, PasswordStoreKeyInsertDto dto);
    /// <returns>404</returns>
    public Task<HttpResponseDto?> RemoveKey(MemberDto memberDto, ulong itemId);
}