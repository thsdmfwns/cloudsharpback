using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;

namespace cloudsharpback.Services.Interfaces
{
    public interface IJWTService
    {
        string WriteAccessToken(MemberDto data);
        string WriteRefreshToken(MemberDto data);
        bool TryValidateAccessToken(string token, out MemberDto? member);
        bool TryValidateRefreshToken(string token, out ulong? memberId);
    }
}
