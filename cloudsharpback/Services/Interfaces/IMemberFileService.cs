using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.FIle;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Models.Ticket;

namespace cloudsharpback.Services.Interfaces
{
    public interface IMemberFileService
    {
        HttpResponseDto? GetFiles(MemberDto memberDto, string? path, out List<FileInfoDto> files, bool onlyDir = false);
        HttpResponseDto? GetFile(MemberDto member, string path, out FileInfoDto? fileDto);
        HttpResponseDto? DeleteFile(MemberDto member, string path, out List<FileInfoDto> fileDto);
        HttpResponseDto? GetDownloadTicket(MemberDto member, string targetPath, out DownloadTicket? ticket, bool isView = false);
        HttpResponseDto? GetUploadTicket(MemberDto member, FileUploadRequestDto uploadRequestDto, out UploadTicket? ticketValue);
        /// <returns> 404 => Root Directory not found, 409 => Directory already exist</returns>
        HttpResponseDto? MakeDirectory(MemberDto memberDto, string? targetPath, string dirName,
            out List<FileInfoDto> fileDtos);
        /// <returns> 404 => Root Directory not found</returns>
        HttpResponseDto? RemoveDirectory(MemberDto memberDto, string targetPath, out List<FileInfoDto> fileDtos);
    }
}