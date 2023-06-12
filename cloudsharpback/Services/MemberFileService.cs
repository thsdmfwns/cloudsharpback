using cloudsharpback.Models;
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

        private string MemberDirectory(string directoryId) => _pathStore.MemberDirectory(directoryId);

        public void MakeBaseDirectory(MemberDto memberDto)
        {
            try
            {
                if (Directory.Exists(MemberDirectory(memberDto.Directory)))
                {
                    return;
                }

                string SubPath(string foldername) => Path.Combine(MemberDirectory(memberDto.Directory), foldername);
                Directory.CreateDirectory(SubPath("Download"));
                Directory.CreateDirectory(SubPath("Music"));
                Directory.CreateDirectory(SubPath("Video"));
                Directory.CreateDirectory(SubPath("Document"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to make template directory",
                });
            }

        }

        public HttpResponseDto? GetFiles(MemberDto memberDto, string? path, out List<FileDto>? files)
        {
            try
            {
                files = null;
                if (!Directory.Exists(MemberDirectory(memberDto.Directory)))
                {
                    MakeBaseDirectory(memberDto);
                }

                var dirPath = Path.Combine(MemberDirectory(memberDto.Directory), path ?? string.Empty);
                var targetDir = new DirectoryInfo(dirPath);
                if (!targetDir.Exists)
                {
                    return new HttpResponseDto()
                    {
                        HttpCode = 404,
                        Message = "Directory Not Found"
                    };
                }
                files = GetFileDtos(memberDto, targetDir);;
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

        private List<FileDto> GetFileDtos(MemberDto memberDto, DirectoryInfo targetDirectoryInfo)
        {
            List<FileDto> fileDtos = new();
            targetDirectoryInfo.GetDirectories().ToList()
                .ForEach(x => fileDtos.Add(FileDto.FromDirectoryInfo(x, MemberDirectory(memberDto.Directory))));
            targetDirectoryInfo.GetFiles().ToList()
                .ForEach(x => fileDtos.Add(FileDto.FromFileInfo(x, MemberDirectory(memberDto.Directory))));
            return fileDtos;
        }

        public HttpResponseDto? GetFile(MemberDto member, string path, out FileDto? fileDto)
        {
            try
            {
                fileDto = null;
                var filepath = Path.Combine(MemberDirectory(member.Directory), path);
                if (!FileExist(filepath))
                {
                    return new HttpResponseDto()
                    {
                        HttpCode = 404,
                        Message = "File Not Found"
                    };
                }
                fileDto = FileDto.FromFileInfo(new FileInfo(filepath), MemberDirectory(member.Directory));
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

        bool FileExist(string filePath) => System.IO.File.Exists(filePath);

        public HttpResponseDto? DeleteFile(MemberDto member, string path, out List<FileDto>? fileDtos)
        {
            try
            {
                fileDtos = null;
                var filepath = Path.Combine(MemberDirectory(member.Directory), path);
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
        
        public HttpResponseDto? CheckBeforeDownloadTicketAdd(MemberDto member, string targetPath, bool isView = false)
        {
            try
            {
                var target = Path.Combine(MemberDirectory(member.Directory), targetPath);
                if (!FileExist(target))
                {
                    return new HttpResponseDto() { HttpCode = 404, Message = "file not found" };
                }
                if (isView && !MimeTypeUtil.CanViewInFront(Path.GetExtension(targetPath)))
                {
                    return new HttpResponseDto() { HttpCode = 415, Message = "file can not view" };
                }
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

        public HttpResponseDto? CheckBeforeUploadTicketAdd(MemberDto member, FileUploadDto uploadDto)
        {
            var targetDir = Path.Combine(MemberDirectory(member.Directory), uploadDto.UploadDirectory ?? string.Empty);
            if (!Directory.Exists(targetDir))
            {
                return new HttpResponseDto() { HttpCode = 404, Message = "Directory not found" };
            }
            var target = Path.Combine(targetDir, uploadDto.FileName);
            if (FileExist(target))
            {
                return new HttpResponseDto() { HttpCode = 409, Message = "File with the same name already exists" };
            }
            return null;
        }
        
        /// <returns> 404 => Root Directory not found, 409 => Directory already exist</returns>
        public HttpResponseDto? MakeDirectory(MemberDto memberDto, string? targetPath, string dirName, out List<FileDto>? fileDtos)
        {
            try
            {
                fileDtos = null;
                var targetDirPath = Path.Combine(MemberDirectory(memberDto.Directory), targetPath ?? string.Empty);
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
        public HttpResponseDto? RemoveDirectory(MemberDto memberDto, string targetPath, out List<FileDto>? fileDtos)
        {
            try
            {
                fileDtos = null;
                if (string.IsNullOrEmpty(targetPath.Trim()) || targetPath.Equals("/"))
                {
                    return new HttpResponseDto() { HttpCode = 400 };
                }
                var targetDirPath = Path.Combine(MemberDirectory(memberDto.Directory), targetPath);
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
