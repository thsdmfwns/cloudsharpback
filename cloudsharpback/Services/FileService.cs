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
using System.IO.Compression;
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

        public void MakeTemplateDirectory(string directoryId)
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to make template directory",
                });
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
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to delete file",
                });
            }

        }


        // old codes
        /*
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
                throw new HttpErrorException(new HttpErrorDto
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
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to download file",
                });
            }
        }
        */

        Dictionary<Guid, (DateTime expireTime, string target)> DownloadTokens = new Dictionary<Guid, (DateTime expireTime, string target)>();

        /// <returns>404 : file not found, 409 : try again</returns>
        public HttpErrorDto? GetDownloadToken(MemberDto member, string targetPath, out Guid? token)
        {
            token = null;
            var target = Path.Combine(userPath(member.Directory), targetPath);
            if (!FileExist(target))
            {
                return new HttpErrorDto() { ErrorCode = 404, Message = "file not found" };
            }
            token = Guid.NewGuid();
            var expireTime = DateTime.Now.AddMinutes(1);
            if (!DownloadTokens.TryAdd(token.Value, (expireTime, target)))
            {
                return new HttpErrorDto() { ErrorCode = 409, Message = "try again" };
            }
            return null;
        }

        /// <returns>404 : file not found, 409 : try again</returns>
        public HttpErrorDto? GetViewToken(MemberDto member, string targetPath, out Guid? token)
        {
            token = null;
            var target = Path.Combine(userPath(member.Directory), targetPath);
            if (!FileExist(target))
            {
                return new HttpErrorDto() { ErrorCode = 404, Message = "file not found" };
            }
            if (!MimeTypeUtil.CanViewInFront(Path.GetExtension(targetPath)))
            {
                return new HttpErrorDto() { ErrorCode = 415, Message = "file can not view in html" };
            }
            token = Guid.NewGuid();
            var expireTime = DateTime.Now.AddHours(1);
            if (!DownloadTokens.TryAdd(token.Value, (expireTime, target)))
            {
                return new HttpErrorDto() { ErrorCode = 409, Message = "try again" };
            }
            return null;
        }

        /// <returns>500 : server error , 403 : bad token, 410 : expire, 404 : file not found</returns>
        public HttpErrorDto? DownloadFile(Guid downloadToken, out FileStream? fileStream)
        {
            try
            {
                fileStream = null;
                if (!DownloadTokens.Remove(downloadToken, out var item))
                {
                    return new HttpErrorDto() { ErrorCode = 403, Message = "bad token" };
                }
                if (item.expireTime < DateTime.Now)
                {
                    DownloadTokens.Where(x => x.Value.expireTime < DateTime.Now).ToList().ForEach(x => DownloadTokens.Remove(x.Key));
                    return new HttpErrorDto() { ErrorCode = 410, Message = "expire token" };
                }
                if (!FileExist(item.target))
                {
                    return new HttpErrorDto() { ErrorCode = 404, Message = "file not found" };
                }
                fileStream = new FileStream(item.target, FileMode.Open, FileAccess.Read);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to download file",
                });
            }
        }

        /// <returns>500 : server error , 403 : bad token, 410 : expire, 404 : file not found</returns>
        public HttpErrorDto? ViewFile(Guid downloadToken, out FileStream? fileStream)
        {
            try
            {
                fileStream = null;
                if (!DownloadTokens.TryGetValue(downloadToken, out var item))
                {
                    return new HttpErrorDto() { ErrorCode = 403, Message = "bad token" };
                }
                if (item.expireTime < DateTime.Now)
                {
                    DownloadTokens.Where(x => x.Value.expireTime < DateTime.Now).ToList().ForEach(x => DownloadTokens.Remove(x.Key));
                    return new HttpErrorDto() { ErrorCode = 410, Message = "expire token" };
                }
                if (!FileExist(item.target))
                {
                    return new HttpErrorDto() { ErrorCode = 404, Message = "file not found" };
                }
                fileStream = new FileStream(item.target, FileMode.Open, FileAccess.Read);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to View file",
                });
            }
        }

        public HttpErrorDto? ViewZip(MemberDto member, string target, out List<ZipEntryDto>? zipEntries)
        {
            try
            {
                zipEntries = null;
                if (Path.GetExtension(target) != ".zip")
                {
                    return new HttpErrorDto() { ErrorCode = 415, Message = "bad file type" };
                }
                var filepath = Path.Combine(userPath(member.Directory), target);
                if (!FileExist(filepath))
                {
                    return new HttpErrorDto() { ErrorCode = 404, Message = "file not found" };
                }
                using ZipArchive archive = ZipFile.OpenRead(filepath);
                var resp = archive.Entries.Select(ZipEntryDto.FromEntry).ToList();
                if (resp is null)
                {
                    return new HttpErrorDto() { ErrorCode = 400, Message = "zip is encrypted" };
                }
                zipEntries = resp;
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to View zip",
                });
            }
        }
    }
}
