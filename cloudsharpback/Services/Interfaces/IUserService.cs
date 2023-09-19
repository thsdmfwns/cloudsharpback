using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;

namespace cloudsharpback.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> IdCheck(string id);
        Task<(HttpResponseDto? err, MemberDto? result)> Login(LoginDto loginDto);
        Task<HttpResponseDto?> Register(RegisterDto registerDto, ulong role, Guid? directoryId = null);
    }
}