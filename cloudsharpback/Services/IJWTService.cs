using cloudsharpback.Models;
using JsonWebToken;

namespace cloudsharpback.Services
{
    public interface IJWTService
    {
        public string TokenCreate(MemberDto data);
        public bool TryTokenValidation(string token, out Jwt? jwt);
    }
}
