using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IFileService
    {
        void MakeTemplateDirectory(string directoryId);
        List<FileDto> GetFiles(string id, string? path);
        bool GetFile(MemberDto member, string path, out FileDto? fileDto);
        bool DeleteFile(MemberDto member, string path, out FileDto? fileDto);
        /// <returns>404 : file not found, 409 : try again</returns>
        HttpErrorDto? GetDownloadToken(MemberDto member, string targetPath, out Guid? ticketToken);
        /// <returns>404 : file not found, 409 : try again, 415 : can not view</returns>
        HttpErrorDto? GetViewToken(MemberDto member, string targetPath, out Guid? ticketToken);
        /// <returns>500 : server error , 403 : bad token, 410 : expire, 404 : file not found</returns>
        HttpErrorDto? GetFileStream(Guid ticketToken, out FileStream? fileStream);
    }
}