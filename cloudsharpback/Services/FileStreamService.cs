using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;

namespace cloudsharpback.Services;

public class FileStreamService : IFileStreamService
{
    private readonly IPathStore _pathStore;
    private readonly ILogger _logger;

    public FileStreamService(IPathStore pathStore, ILogger<IFileStreamService> logger)
    {
        _pathStore = pathStore;
        _logger = logger;
    }

    private string MemberDirectory(string directoryId) => _pathStore.MemberDirectory(directoryId);
    private bool FileExist(string filePath) => System.IO.File.Exists(filePath);
    
    public HttpErrorDto? GetFileStream(Ticket ticket, out FileStream? fileStream)
    {
        try
        {
            fileStream = null;
            if (ticket.Target is null ||
                (ticket.TicketType != TicketType.Download && ticket.TicketType != TicketType.ViewFile) ||
                ticket.Target is not DownloadToken file)
            {
                return new HttpErrorDto() { ErrorCode = 404, Message = "wrong ticket" };
            }
            var targetPath = Path.Combine(MemberDirectory(file.FileDirectory), file.FIlePath);
            if (!FileExist(targetPath))
            {
                return new HttpErrorDto() { ErrorCode = 404, Message = "file not found" };
            }
            fileStream = new FileStream(targetPath, FileMode.Open, FileAccess.Read);
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