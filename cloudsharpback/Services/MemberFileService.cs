using System.Text.RegularExpressions;
using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.FIle;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;

namespace cloudsharpback.Services
{
    public class MemberFileService : IMemberFileService
    {
        private readonly ILogger _logger;
        private readonly IPathStore _pathStore;

        public MemberFileService(ILogger<IMemberFileService> logger, IPathStore pathStore)
        {
            _logger = logger;
            _pathStore = pathStore;
        }

        private string MemberDirectory(string directoryId)
        {
            var path = _pathStore.MemberDirectory(directoryId);
            if (!Directory.Exists(path))
            {
                MakeBaseDirectory(path);
            }

            return path;
        }
        
        private void MakeBaseDirectory(string memberDirectory)
        {
            string SubPath(string foldername) => Path.Combine(memberDirectory, foldername);
            Directory.CreateDirectory(SubPath("Download"));
            Directory.CreateDirectory(SubPath("Music"));
            Directory.CreateDirectory(SubPath("Video"));
            Directory.CreateDirectory(SubPath("Document"));
        }

        private string GetTargetFullPath(string memberDirectoryId, string? targetPath)
        {
            var memberDirectory = MemberDirectory(memberDirectoryId);
            targetPath ??= string.Empty;
            if (Path.IsPathRooted(targetPath))
            {
                throw new ArgumentException("Target path is rooted");
            }
            return Path.Combine(memberDirectory, targetPath);
        }

        private string GetTargetFullPath(MemberDto memberDto, string? targetPath)
            => GetTargetFullPath(memberDto.Directory, targetPath);

        public HttpResponseDto? GetFiles(MemberDto memberDto, string? path, out List<FileInfoDto> files, bool onlyDir = false)
        {
            try
            {
                files = new List<FileInfoDto>();
                var dirPath = GetTargetFullPath(memberDto, path);
                var targetDir = new DirectoryInfo(dirPath);
                if (!targetDir.Exists)
                {
                    return new HttpResponseDto()
                    {
                        HttpCode = 404,
                        Message = "Directory Not Found"
                    };
                }
                files = GetFileDtos(memberDto, targetDir, onlyDir);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to GetFiles",
                });
            }
            
        }

        private List<FileInfoDto> GetFileDtos(MemberDto memberDto, DirectoryInfo targetDirectoryInfo, bool onlyDir = false)
        {
            List<FileInfoDto> fileDtos = new();
            var memberDir = MemberDirectory(memberDto.Directory);
            targetDirectoryInfo.GetDirectories().ToList()
                .ForEach(x => fileDtos.Add(FileInfoDto.FromDirectoryInfo(x, memberDir)));
            if (onlyDir)
            {
                return fileDtos;
            }
            targetDirectoryInfo.GetFiles().ToList()
                .ForEach(x => fileDtos.Add(FileInfoDto.FromFileInfo(x, memberDir)));
            return fileDtos;
        }
        
        public HttpResponseDto? GetFile(MemberDto member, string path, out FileInfoDto? fileDto)
        {
            try
            {
                fileDto = null;
                var filepath = GetTargetFullPath(member, path);
                if (!FileExist(filepath))
                {
                    return new HttpResponseDto()
                    {
                        HttpCode = 404,
                        Message = "File Not Found"
                    };
                }
                fileDto = FileInfoDto.FromFileInfo(new FileInfo(filepath), MemberDirectory(member.Directory));
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to GetFile",
                });
            }
        }

        bool FileExist(string filePath) => File.Exists(filePath);

        public HttpResponseDto? DeleteFile(MemberDto member, string path, out List<FileInfoDto> fileDtos)
        {
            try
            {
                fileDtos = new List<FileInfoDto>();
                var filepath = GetTargetFullPath(member, path);
                if (!FileExist(filepath))
                {
                    return new HttpResponseDto()
                    {
                        HttpCode = 404,
                        Message = "File Not Found"
                    };
                }
                var file = new FileInfo(filepath);
                var targetDir = new DirectoryInfo(file.Directory!.FullName);
                if (!targetDir.Exists)
                {
                    return new HttpResponseDto()
                    {
                        HttpCode = 404,
                        Message = "Directory Not Found"
                    };
                }
                File.Delete(filepath);
                fileDtos = GetFileDtos(memberDto: member, targetDir);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to delete file",
                });
            }
        }
        
        public HttpResponseDto? GetDownloadTicketValue(MemberDto member, string targetPath, out FileDownloadTicketValue? ticketValue, bool isView = false)
        {
            try
            {
                ticketValue = null;
                var targetFilePath = GetTargetFullPath(member, targetPath);;
                if (!FileExist(targetFilePath))
                {
                    return new HttpResponseDto() { HttpCode = 404, Message = "file not found" };
                }
                if (isView && !MimeTypeUtil.CanViewInFront(Path.GetExtension(targetPath)))
                {
                    return new HttpResponseDto() { HttpCode = 415, Message = "file can not view" };
                }
                ticketValue = new FileDownloadTicketValue()
                {
                    FileDownloadType = isView ? FileDownloadType.View : FileDownloadType.Download,
                    TargetFilePath = targetFilePath
                };
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to CheckBeforeDownloadTicketAdd",
                });
            }
            
        }

        public HttpResponseDto? GetUploadTicketValue(MemberDto member, FileUploadRequestDto uploadRequestDto, out FileUploadTicketValue? ticketValue)
        {
            ticketValue = null;
            var targetDir = GetTargetFullPath(member, uploadRequestDto.UploadDirectory);;
            if (!Directory.Exists(targetDir))
            {
                return new HttpResponseDto() { HttpCode = 404, Message = "Directory not found" };
            }
            var target = Path.Combine(targetDir, uploadRequestDto.FileName);
            if (FileExist(target))
            {
                return new HttpResponseDto() { HttpCode = 409, Message = "File with the same name already exists" };
            }
            ticketValue = new FileUploadTicketValue()
            {
                FileName = uploadRequestDto.FileName,
                UploadDirectoryPath = targetDir
            };
            return null;
        }
        
        /// <returns> 404 => Root Directory not found, 409 => Directory already exist</returns>
        public HttpResponseDto? MakeDirectory(MemberDto memberDto, string? targetPath, string dirName, out List<FileInfoDto> fileDtos)
        {
            try
            {
                fileDtos = new List<FileInfoDto>();
                if (!Regex.IsMatch(dirName, "^[a-zA-Z0-9_-]+$"))
                {
                    return new HttpResponseDto() { HttpCode = 400, Message = "Bad Directory Name" };
                }
                var targetDirPath = GetTargetFullPath(memberDto, targetPath);;
                var makingDirPath = Path.Combine(targetDirPath, dirName);
                var targetDir = new DirectoryInfo(targetDirPath);
                if (!targetDir.Exists)
                {
                    return new HttpResponseDto() { HttpCode = 404, Message = "Root Directory not found" };
                }
                if (Directory.Exists(makingDirPath))
                {
                    return new HttpResponseDto() { HttpCode = 409, Message = "Directory already exist" };
                }
                Directory.CreateDirectory(makingDirPath);
                fileDtos = GetFileDtos(memberDto, targetDir);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to MakeDirectory",
                });
            }
        }

        /// <returns> 404 => Root Directory not found</returns>
        public HttpResponseDto? RemoveDirectory(MemberDto memberDto, string targetPath, out List<FileInfoDto> fileDtos)
        {
            try
            {
                fileDtos = new List<FileInfoDto>();
                if (string.IsNullOrEmpty(targetPath.Trim()) || targetPath.Equals("/"))
                {
                    return new HttpResponseDto() { HttpCode = 400 };
                }
                var targetDirPath = GetTargetFullPath(memberDto, targetPath);;
                var targetdir = new DirectoryInfo(targetDirPath);
                if (!targetdir.Exists)
                {
                    return new HttpResponseDto() { HttpCode = 404, Message = "Directory not found" };
                }
                targetdir.Delete(true);
                if (targetdir.Parent is null)
                {
                    return null;
                }
                fileDtos = GetFileDtos(memberDto, targetdir.Parent);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to RemoveDirectory",
                });
            }
        }
    }
}
