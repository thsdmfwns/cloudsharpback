using cloudsharpback.Models;
using JsonWebToken;

namespace cloudsharpback.Services.Interfaces
{
    public interface IJWTService
    {
        public bool TryTokenCreate(MemberDto data, out string? token);
        public bool TryTokenValidation(string token, out MemberDto? member);
    }
}
