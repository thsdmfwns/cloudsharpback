using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IMemberService
    {
        Task<(HttpResponseDto? err, MemberDto? result)> GetMemberById(ulong id);
        /// <returns>404 : fail to login</returns>
        Task<HttpResponseDto?> UploadProfileImage(IFormFile imageFile, MemberDto member);
        HttpResponseDto? DownloadProfileImage(string profileImage, out FileStream? fileStream, out string? contentType);
        Task<HttpResponseDto?> UpadteNickname(MemberDto member, string changeNick);
        Task<HttpResponseDto?> UpadteEmail(MemberDto member, string changeEmail);
        Task<(HttpResponseDto? err, bool result)> CheckPassword(MemberDto member, string password);
        Task<HttpResponseDto?> UpdatePassword(MemberDto member, UpadtePasswordDto requset);
    }
}