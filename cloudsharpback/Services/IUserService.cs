using cloudsharpback.Models;

namespace cloudsharpback.Services
{
    public interface IUserService
    {
        public bool TryLogin(LoginDto loginDto, out string token);
        public bool TryRegister(RegisterDto registerDto);

    }
}
