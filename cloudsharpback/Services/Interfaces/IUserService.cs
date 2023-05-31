using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> IdCheck(string id);
        Task<(HttpResponseDto? err, MemberDto? result)> Login(LoginDto loginDto);
        Task<(HttpResponseDto? err, string? directoryId)> Register(RegisterDto registerDto, ulong role);
    }
}