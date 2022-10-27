using cloudsharpback.Models;
using JsonWebToken;

namespace cloudsharpback.Services.Interfaces
{
    public interface IJWTService
    {
        public string WriteToken(MemberDto data);
        public bool TryValidateToken(string token, out MemberDto? member);
    }
}
