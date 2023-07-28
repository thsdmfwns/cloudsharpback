using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;

namespace cloudsharpback.Repository.Interface;

public interface IMemberRepository
{
    Task<MemberDto?> GetMemberById(ulong id);
    Task<MemberDto?> GetMemberByLoginId(string id);
    Task<bool> TryUpdateMemberProfileImage(ulong id, string imageFileName);
    Task<bool> TryUpdateMemberNickname(ulong id, string nickname);
    Task<bool> TryUpdateMemberEmail(ulong id, string email);
    Task<string?> GetMemberPasswordHashById(ulong id);
    Task<string?> GetMemberPasswordHashByLoginId(string id);
    Task<bool> TryLoginIdDuplicate(string id);
    Task<bool> TryUpdateMemberPassword(ulong id, string password);
    Task<bool> TryAddMember(RegisterDto registerDto, ulong role);
}