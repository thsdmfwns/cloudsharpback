using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IMemberFileService
    {
        void MakeTemplateDirectory(string directoryId);
        List<FileDto> GetFiles(string id, string? path);
        bool GetFile(MemberDto member, string path, out FileDto? fileDto);
        bool DeleteFile(MemberDto member, string path, out FileDto? fileDto);
        HttpErrorDto? CheckBeforeTicketAdd(MemberDto member, string targetPath, bool isView = false);
        
    }
}