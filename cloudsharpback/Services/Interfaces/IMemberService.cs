using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;

namespace cloudsharpback.Services.Interfaces
{
    public interface IMemberService
    {
        /// <returns>404 : fail to login</returns>
        Task<(HttpResponseDto? err, MemberDto? result)> GetMemberById(ulong id);
        /// <returns>415 : bad type, 409 : try again, 404: member not found</returns>
        Task<HttpResponseDto?> UploadProfileImage(IFormFile imageFile, MemberDto member, Guid? profileId = null);
        /// <returns>404 : file not found</returns>
        HttpResponseDto? DownloadProfileImage(string profileImage, out FileStream? fileStream, out string? contentType);
        /// <returns>404 : member not found</returns>
        Task<HttpResponseDto?> UpdateNickname(MemberDto member, string changeNick);
        /// <returns>404 : member not found, 400 : bad email</returns>
        Task<HttpResponseDto?> UpdateEmail(MemberDto member, string changeEmail);
        /// <returns>404 : member not found</returns>
        Task<(HttpResponseDto? err, bool result)> CheckPassword(MemberDto member, string password);
        /// <returns>404 : member not found, 400 : wrong password</returns>
        Task<HttpResponseDto?> UpdatePassword(MemberDto member, UpadtePasswordDto requset);
    }
}