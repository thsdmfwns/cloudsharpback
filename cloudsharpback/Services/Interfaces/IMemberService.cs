using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;

namespace cloudsharpback.Services.Interfaces
{
    public interface IMemberService
    {
        Task<(HttpResponseDto? err, MemberDto? result)> GetMemberById(ulong id);
        /// <returns>404 : fail to login</returns>
        Task<HttpResponseDto?> UploadProfileImage(IFormFile imageFile, MemberDto member, Guid? profileId = null);
        HttpResponseDto? DownloadProfileImage(string profileImage, out FileStream? fileStream, out string? contentType);
        Task<HttpResponseDto?> UpdateNickname(MemberDto member, string changeNick);
        Task<HttpResponseDto?> UpdateEmail(MemberDto member, string changeEmail);
        Task<(HttpResponseDto? err, bool result)> CheckPassword(MemberDto member, string password);
        Task<HttpResponseDto?> UpdatePassword(MemberDto member, UpadtePasswordDto requset);
    }
}