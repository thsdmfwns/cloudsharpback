﻿using cloudsharpback.Models;

namespace cloudsharpback.Services
{
    public interface IFileService
    {
        bool TryMakeTemplateDirectory(string directoryId);
        List<FileDto> GetFiles(string id, string? path);
        bool GetFile(MemberDto member, string path, out FileDto? fileDto);
        Task<bool> UploadFile(IFormFile file, MemberDto member, string? path);
        bool DownloadFile(MemberDto member, string path, out FileStream? fileStream);
        bool DeleteFile(MemberDto member, string path, out FileDto? fileDto);
    }
}