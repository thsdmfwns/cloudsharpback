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

        public void MakeTemplateDirectory(string directoryId)
        {
            try
            {
                string SubPath(string foldername) => Path.Combine(MemberDirectory(directoryId), foldername);
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

        public List<FileDto> GetFiles(string id, string? path)
        {
            List<FileDto> fileDtos = new();
            var dir = new DirectoryInfo(Path.Combine(MemberDirectory(id), path ?? string.Empty));
            foreach (var fol in dir.GetDirectories())
            {
                fileDtos.Add(new FileDto
                {
                    Name = fol.Name,
                    FileType = FileType.FOLDER,
                    Path = fol.FullName.Substring(MemberDirectory(id).Length + 1),
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
                    Path = file.FullName.Substring(MemberDirectory(id).Length + 1),
                });
            }
            return fileDtos;
        }

        public bool GetFile(MemberDto member, string path, out FileDto? fileDto)
        {
            var filepath = Path.Combine(MemberDirectory(member.Directory), path);
            if (!FileExist(filepath))
            {
                fileDto = null;
                return false;
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
            return true;
        }

        bool FileExist(string filePath) => System.IO.File.Exists(filePath);

        public bool DeleteFile(MemberDto member, string path, out FileDto? fileDto)
        {
            try
            {
                var filepath = Path.Combine(MemberDirectory(member.Directory), path);
                if (!FileExist(filepath))
                {
                    fileDto = null;
                    return false;
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

        /// <returns>404 : file not found, 409 : try again</returns>
        public HttpErrorDto? GetDownloadTicket(MemberDto member, string targetPath, string reqIp, out Ticket? ticket)
        {
            ticket = null;
            var target = Path.Combine(MemberDirectory(member.Directory), targetPath);
            if (!FileExist(target))
            {
                return new HttpErrorDto() { ErrorCode = 404, Message = "file not found" };
            }
            ticket = new Ticket(member.Directory, TicketType.Download, reqIp, true, target);
            return null;
        }

        /// <returns>404 : file not found, 409 : try again</returns>
        public HttpErrorDto? GetViewTicket(MemberDto member, string targetPath, string reqIp, out Ticket? ticket)
        {
            ticket = null;
            var target = Path.Combine(MemberDirectory(member.Directory), targetPath);
            if (!FileExist(target))
            {
                return new HttpErrorDto() { ErrorCode = 404, Message = "file not found" };
            }
            if (!MimeTypeUtil.CanViewInFront(Path.GetExtension(targetPath)))
            {
                return new HttpErrorDto() { ErrorCode = 415, Message = "file can not view" };
            }
            ticket = new Ticket(member.Directory, TicketType.Download, reqIp, true, target);
            return null;
        }
    }
}
