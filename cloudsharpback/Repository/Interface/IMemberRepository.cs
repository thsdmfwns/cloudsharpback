using cloudsharpback.Models;

namespace cloudsharpback.Repository.Interface;

public interface IMemberRepository
{
    Task<MemberDto?> GetMemberById(ulong id);
    Task<bool> TryUpdateMemberProfileImage(ulong id, string imageFileName);
    Task<bool> TryUpdateMemberNickname(ulong id, string nickname);
    Task<string?> GetMemberPasswordHashById(ulong id);
    Task<bool> TryUpdateMemberPassword(ulong id, string password);
    Task<bool> TryAddMember(RegisterDto registerDto, uint role);
}