using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> IdCheck(string id);
        /// <returns>404 : fail to login</returns>
        Task<(HttpErrorDto? err, MemberDto? result)> Login(LoginDto loginDto);
        /// <returns>404 : bad json </returns>
        Task<(HttpErrorDto? err, string? directoryId)> Register(RegisterDto registerDto, ulong role);
    }
}