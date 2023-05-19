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
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to make template directory",
                });
            }

        }

        public HttpErrorDto? GetFiles(MemberDto memberDto, string? path, out List<FileDto>? files)
        {
            files = null;
            if (!Directory.Exists(MemberDirectory(memberDto.Directory)))
            {
                MakeBaseDirectory(memberDto);
            }

            var dirPath = Path.Combine(MemberDirectory(memberDto.Directory), path ?? string.Empty);
            if (!Directory.Exists(dirPath))
            {
                var err = new HttpErrorDto()
                {
                    ErrorCode = 404,
                    Message = "Directory Not Found"
                };
                return err;
            }
            List<FileDto> fileDtos = new();
            var dir = new DirectoryInfo(dirPath);
            foreach (var fol in dir.GetDirectories())
            {
                fileDtos.Add(new FileDto
                {
                    Name = fol.Name,
                    FileType = FileType.FOLDER,
                    Path = fol.FullName.Substring(MemberDirectory(memberDto.Directory).Length + 1),
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
                    Path = file.FullName.Substring(MemberDirectory(memberDto.Directory).Length + 1),
                });
            }

            files = fileDtos;
            return null;
        }

        public HttpErrorDto? GetFile(MemberDto member, string path, out FileDto? fileDto)
        {
            fileDto = null;
            var filepath = Path.Combine(MemberDirectory(member.Directory), path);
            if (!FileExist(filepath))
            {
                return new HttpErrorDto()
                {
                    ErrorCode = 404,
                    Message = "File Not Found"
                };
            }
            var file = new FileInfo(filepath);
            fileDto = new FileDto
            {
                Name = file.Name,
                FileType = FileType.FILE,
                Extention = file.Extension,
                LastWriteTime = file.LastWriteTime.ToUniversalTime().Ticks,
                Size = (ulong?)file.Length,
                Path = file.FullName.Substring(MemberDirectory(member.Directory).Length + 1),
            };
            return null;
        }

        bool FileExist(string filePath) => System.IO.File.Exists(filePath);

        public HttpErrorDto? DeleteFile(MemberDto member, string path, out FileDto? fileDto)
        {
            try
            {
                fileDto = null;
                var filepath = Path.Combine(MemberDirectory(member.Directory), path);
                if (!FileExist(filepath))
                {
                    return new HttpErrorDto()
                    {
                        ErrorCode = 404,
                        Message = "File Not Found"
                    };
                }

                var file = new FileInfo(filepath);
                fileDto = new FileDto
                {
                    Name = file.Name,
                    FileType = FileType.FILE,
                    Extention = file.Extension,
                    LastWriteTime = file.LastWriteTime.ToFileTimeUtc(),
                    Size = (ulong?)file.Length
                };
                System.IO.File.Delete(filepath);
                return null;
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
        
        public HttpErrorDto? CheckBeforeDownloadTicketAdd(MemberDto member, string targetPath, bool isView = false)
        {
            var target = Path.Combine(MemberDirectory(member.Directory), targetPath);
            if (!FileExist(target))
            {
                return new HttpErrorDto() { ErrorCode = 404, Message = "file not found" };
            }
            if (isView && !MimeTypeUtil.CanViewInFront(Path.GetExtension(targetPath)))
            {
                return new HttpErrorDto() { ErrorCode = 415, Message = "file can not view" };
            }
            return null;
        }

        public HttpErrorDto? CheckBeforeUploadTicketAdd(MemberDto member, FileUploadDto uploadDto)
        {
            var targetDir = Path.Combine(MemberDirectory(member.Directory), uploadDto.FilePath ?? string.Empty);
            if (!Directory.Exists(targetDir))
            {
                return new HttpErrorDto() { ErrorCode = 404, Message = "Directory not found" };
            }
            var target = Path.Combine(targetDir, uploadDto.FileName);
            if (FileExist(target))
            {
                return new HttpErrorDto() { ErrorCode = 409, Message = "File with the same name already exists" };
            }
            return null;
        }
    }
}
