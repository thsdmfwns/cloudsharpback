using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IMemberFileService
    {
        HttpResponseDto? GetFiles(MemberDto memberDto, string? path, out List<FileDto>? files);
        HttpResponseDto? GetFile(MemberDto member, string path, out FileDto? fileDto);
        HttpResponseDto? DeleteFile(MemberDto member, string path, out FileDto? fileDto);
        HttpResponseDto? CheckBeforeDownloadTicketAdd(MemberDto member, string targetPath, bool isView = false);
        HttpResponseDto? CheckBeforeUploadTicketAdd(MemberDto member, FileUploadDto uploadDto);
        
    }
}