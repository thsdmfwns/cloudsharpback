using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IMemberFileService
    {
        HttpResponseDto? GetFiles(MemberDto memberDto, string? path, out List<FileDto>? files);
        HttpResponseDto? GetFile(MemberDto member, string path, out FileDto? fileDto);
        HttpResponseDto? DeleteFile(MemberDto member, string path, out List<FileDto>? fileDto);
        HttpResponseDto? CheckBeforeDownloadTicketAdd(MemberDto member, string targetPath, bool isView = false);
        HttpResponseDto? CheckBeforeUploadTicketAdd(MemberDto member, FileUploadDto uploadDto);
        /// <returns> 404 => Root Directory not found, 409 => Directory already exist</returns>
        HttpResponseDto? MakeDirectory(MemberDto memberDto, string? targetPath, string dirName,
            out List<FileDto>? fileDtos);
        /// <returns> 404 => Root Directory not found</returns>
        HttpResponseDto? RemoveDirectory(MemberDto memberDto, string targetPath, out List<FileDto>? fileDtos);
    }
}