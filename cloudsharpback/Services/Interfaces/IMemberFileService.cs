using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IMemberFileService
    {
        void MakeBaseDirectory(MemberDto memberDto);
        HttpErrorDto? GetFiles(MemberDto memberDto, string? path, out List<FileDto>? files);
        HttpErrorDto? GetFile(MemberDto member, string path, out FileDto? fileDto);
        HttpErrorDto? DeleteFile(MemberDto member, string path, out FileDto? fileDto);
        HttpErrorDto? CheckBeforeTicketAdd(MemberDto member, string targetPath, bool isView = false);
        
    }
}