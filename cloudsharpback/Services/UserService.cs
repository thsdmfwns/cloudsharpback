using cloudsharpback.Models;

namespace cloudsharpback.Services
{
    public class UserService : IUserService
    {
        private readonly IJWTService jWTService;
        public UserService(IJWTService jWTService)
        {
            this.jWTService = jWTService;
        }

        public bool TryLogin(LoginDto loginDto, out string token)
        {
            throw new NotImplementedException();
        }

        public bool TryRegister(RegisterDto registerDto)
        {
            throw new NotImplementedException();
        }
    }
}
