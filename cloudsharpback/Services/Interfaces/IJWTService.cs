using cloudsharpback.Models;

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
