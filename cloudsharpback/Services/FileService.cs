using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
using Dapper;
using JsonWebToken;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System.Diagnostics.Metrics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace cloudsharpback.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger _logger;
        private string DirectoryPath;

        public FileService(IConfiguration configuration, ILogger<IFileService> logger)
        {
            DirectoryPath = configuration["File:DirectoryPath"];
            _logger = logger;
        }

        string userPath(string directoryId) => Path.Combine(DirectoryPath, directoryId);

        public bool TryMakeTemplateDirectory(string directoryId)
        {
            try
            {
                string path = userPath(directoryId);
                string subPath(string foldername) => Path.Combine(path, foldername);
                Directory.CreateDirectory(path);
                Directory.CreateDirectory(subPath("Download"));
                Directory.CreateDirectory(subPath("Music"));
                Directory.CreateDirectory(subPath("Video"));
                Directory.CreateDirectory(subPath("Document"));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                return false;
            }

        }

        public List<FileDto> GetFiles(string id, string? path)
        {
            List<FileDto> fileDtos = new();
            var dir = new DirectoryInfo(Path.Combine(userPath(id),path ?? string.Empty));
            foreach (var fol in dir.GetDirectories())
            {
                fileDtos.Add(new()
                {
                    Name = fol.Name,
                    FileType = FileType.FOLDER,
                    Path = fol.FullName.Substring(userPath(id).Length + 1),
                });
            }
            foreach (var file in dir.GetFiles())
            {
                fileDtos.Add(new()
                {
                    Name = file.Name,
                    FileType = FileType.FILE,
                    Extention = file.Extension,
                    LastWriteTime = file.LastWriteTime.ToUniversalTime().Ticks,
                    Size = (ulong?)file.Length,
                    Path = file.FullName.Substring(userPath(id).Length + 1),
                });
            }
            return fileDtos;
        }

        public bool GetFile(MemberDto member, string path, out FileDto? fileDto)
        {
            var filepath = Path.Combine(userPath(member.Directory), path);
            if (!FileExist(filepath))
            {
                fileDto = null;
                return false;
            }
            var file = new FileInfo(filepath);
            fileDto = new()
            {
                Name = file.Name,
                FileType = FileType.FILE,
                Extention = file.Extension,
                LastWriteTime = file.LastWriteTime.ToUniversalTime().Ticks,
                Size = (ulong?)file.Length,
                Path = file.FullName.Substring(userPath(member.Directory).Length + 1),
            };
            return true;
        }

        bool FileExist(string filePath) => System.IO.File.Exists(filePath);

        public bool DeleteFile(MemberDto member, string path, out FileDto? fileDto)
        {
            try
            {
                var filepath = Path.Combine(userPath(member.Directory), path);
                if (!FileExist(filepath))
                {
                    fileDto = null;
                    return false;
                }
                var file = new FileInfo(filepath);
                fileDto = new()
                {
                    Name = file.Name,
                    FileType = FileType.FILE,
                    Extention = file.Extension,
                    LastWriteTime = file.LastWriteTime.ToFileTimeUtc(),
                    Size = (ulong?)file.Length
                };
                System.IO.File.Delete(filepath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDetail
                {
                    ErrorCode = 500,
                    Message = "fail to delete file",
                });
            }

        }

        public async Task<bool> UploadFile(IFormFile file, MemberDto member, string? path)
        {
            try
            {
                var filepath = Path.Combine(userPath(member.Directory), path ?? string.Empty, file.FileName);
                if (FileExist(filepath))
                {
                    return false;
                }
                using var stream = System.IO.File.Create(filepath);
                await file.CopyToAsync(stream);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDetail
                {
                    ErrorCode = 500,
                    Message = "fail to upload file",
                });
            }
        }

        public bool DownloadFile(MemberDto member, string path, out FileStream? fileStream)
        {
            try
            {
                var filepath = Path.Combine(userPath(member.Directory), path);
                if (!FileExist(filepath))
                {
                    fileStream = null;
                    return false;
                }
                fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                return fileStream is null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDetail
                {
                    ErrorCode = 500,
                    Message = "fail to download file",
                });
            }
        }

        
    }
}
