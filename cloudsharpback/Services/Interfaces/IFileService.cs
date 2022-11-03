using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IFileService
    {
        void TryMakeTemplateDirectory(string directoryId);
        List<FileDto> GetFiles(string id, string? path);
        bool GetFile(MemberDto member, string path, out FileDto? fileDto);
        Task<bool> UploadFile(IFormFile file, MemberDto member, string? path);
        bool DownloadFile(MemberDto member, string path, out FileStream? fileStream);
        bool DeleteFile(MemberDto member, string path, out FileDto? fileDto);
        /// <returns>404 : file not found, 409 : try again</returns>
        HttpErrorDto? GetDownloadToken(MemberDto member, string targetPath, out Guid? token);
        /// <returns>500 : server error , 403 : bad token, 410 : expire, 404 : file not found</returns>
        HttpErrorDto? DownloadFile(Guid downloadToken, out FileStream? fileStream);
    }
}