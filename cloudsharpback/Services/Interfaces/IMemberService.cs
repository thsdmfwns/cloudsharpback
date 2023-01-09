using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IMemberService
    {
        Task<(HttpErrorDto? err, MemberDto? result)> GetMemberById(ulong id);
        /// <returns>404 : fail to login</returns>
        Task<HttpErrorDto?> UploadProfileImage(IFormFile imageFile, MemberDto member);
        HttpErrorDto? DownloadProfileImage(string profileImage, out FileStream? fileStream, out string? contentType);
        Task<HttpErrorDto?> UpadteNickname(MemberDto member, string changeNick);
        Task<HttpErrorDto?> UpadteEmail(MemberDto member, string changeEmail);
        Task<(HttpErrorDto? err, bool result)> CheckPassword(MemberDto member, string password);
        Task<HttpErrorDto?> UpdatePassword(MemberDto member, UpadtePasswordDto requset);
    }
}