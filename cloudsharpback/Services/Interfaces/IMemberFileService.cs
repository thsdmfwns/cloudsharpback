using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IMemberFileService
    {
        HttpResponseDto? GetFiles(MemberDto memberDto, string? path, out List<FileInfoDto> files);
        HttpResponseDto? GetFile(MemberDto member, string path, out FileInfoDto? fileDto);
        HttpResponseDto? DeleteFile(MemberDto member, string path, out List<FileInfoDto> fileDto);
        HttpResponseDto? GetDownloadTicketValue(MemberDto member, string targetPath, out FileDownloadTicketValue? ticketValue, bool isView = false);
        HttpResponseDto? GetUploadTicketValue(MemberDto member, FileUploadRequestDto uploadRequestDto, out FileUploadTicketValue? ticketValue);
        /// <returns> 404 => Root Directory not found, 409 => Directory already exist</returns>
        HttpResponseDto? MakeDirectory(MemberDto memberDto, string? targetPath, string dirName,
            out List<FileInfoDto> fileDtos);
        /// <returns> 404 => Root Directory not found</returns>
        HttpResponseDto? RemoveDirectory(MemberDto memberDto, string targetPath, out List<FileInfoDto> fileDtos);
    }
}