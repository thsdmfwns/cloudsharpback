using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> IdCheck(string id);
        Task<(ServiceResult response, MemberDto? result)> Login(LoginDto loginDto);
        Task<(ServiceResult response, string? directoryId)> Register(RegisterDto registerDto, ulong role);
    }
}