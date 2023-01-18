using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IJWTService
    {
        string WriteAcessToken(MemberDto data);
        string WriteRefeshToken(MemberDto data);
        bool TryValidateAcessToken(string token, out MemberDto? member);
        bool TryValidateRefeshToken(string token, out ulong? memberId);
    }
}
