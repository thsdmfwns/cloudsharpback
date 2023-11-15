using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;

namespace cloudsharpback.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> IdCheck(string id);
        /// <returns>401 : login fail, 404 : member not found </returns>
        Task<(HttpResponseDto? err, MemberDto? result)> Login(LoginDto loginDto);
        /// <returns>400 </returns>
        Task<HttpResponseDto?> Register(RegisterDto registerDto, ulong role, Guid? directoryId = null);
    }
}