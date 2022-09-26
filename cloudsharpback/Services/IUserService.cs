using cloudsharpback.Models;

namespace cloudsharpback.Services
{
    public interface IUserService
    {
        public bool IdCheck(string id, out string? passwordHash);
        public bool IdCheck(string id);
        public bool TryLogin(LoginDto loginDto, out MemberDto? member);
        public bool TryRegister(RegisterDto registerDto);

    }
}
