using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;

namespace cloudsharpback.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger _logger;
        private readonly IPathStore _pathStore;
        private readonly ITicketStore _ticketStore;

        public FileService(ILogger<IFileService> logger, IPathStore pathStore, ITicketStore ticketStore)
        {
            _logger = logger;
            _pathStore = pathStore;
            _ticketStore = ticketStore;
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
        public HttpErrorDto? GetDownloadToken(MemberDto member, string targetPath, out Guid? ticketToken)
        {
            ticketToken = null;
            var target = Path.Combine(MemberDirectory(member.Directory), targetPath);
            if (!FileExist(target))
            {
                return new HttpErrorDto() { ErrorCode = 404, Message = "file not found" };
            }
            _ticketStore.Add(member, TicketType.Download, out var token, targetPath);
            ticketToken = token;
            return null;
        }

        /// <returns>404 : file not found, 409 : try again</returns>
        public HttpErrorDto? GetViewToken(MemberDto member, string targetPath, out Guid? ticketToken)
        {
            ticketToken = null;
            var target = Path.Combine(MemberDirectory(member.Directory), targetPath);
            if (!FileExist(target))
            {
                return new HttpErrorDto() { ErrorCode = 404, Message = "file not found" };
            }
            if (!MimeTypeUtil.CanViewInFront(Path.GetExtension(targetPath)))
            {
                return new HttpErrorDto() { ErrorCode = 415, Message = "file can not view" };
            }
            _ticketStore.Add(member, TicketType.ViewFile, out var token, targetPath);
            ticketToken = token;
            return null;
        }

        /// <returns>500 : server error , 403 : bad token, 410 : expire, 404 : file not found</returns>
        public HttpErrorDto? GetFileStream(Guid ticketToken, out FileStream? fileStream)
        {
            try
            {
                fileStream = null;
                if (!_ticketStore.TryGet(ticketToken, out var ticket)
                    || ticket is null)
                {
                    return new HttpErrorDto() { ErrorCode = 404, Message = "ticket not found" };
                }
                var targetTicket = Path.Combine(MemberDirectory(ticket.Member.Directory), ticket.Target!);
                
                if (ticket?.Target is null || !FileExist(targetTicket))
                {
                    return new HttpErrorDto() { ErrorCode = 404, Message = "file not found" };
                }
                fileStream = new FileStream(targetTicket, FileMode.Open, FileAccess.Read);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to get filestream",
                });
            }
        }
    }
}
