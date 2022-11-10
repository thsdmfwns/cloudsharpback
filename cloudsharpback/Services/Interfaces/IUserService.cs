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
        /// <returns>415 : bad type, 409 : try again, 404: member not found</returns>
        Task<HttpErrorDto?> UploadProfileImage(IFormFile imageFile, MemberDto member);
        HttpErrorDto? DownloadProfileImage(string profileImage, out FileStream? fileStream, out string? contentType);
    }
}