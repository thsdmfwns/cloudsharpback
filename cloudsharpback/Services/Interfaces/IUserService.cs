using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> IdCheck(string id);
        Task<(HttpErrorDto? err, MemberDto? result)> Login(LoginDto loginDto);
        Task<(HttpErrorDto? err, string? directoryId)> Register(RegisterDto registerDto, ulong role);
    }
}