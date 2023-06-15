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
    private bool FileExist(string filePath) => File.Exists(filePath);
    
    public HttpResponseDto? GetFileStream(Ticket ticket, out FileStream? fileStream)
    {
        try
        {
            fileStream = null;
            if (ticket.Value is null ||
                (ticket.TicketType != TicketType.Download && ticket.TicketType != TicketType.ViewFile) ||
                ticket.Value is not FileDownloadTicketValue downloadToken)
            {
                return new HttpResponseDto() { HttpCode = 404, Message = "wrong ticket" };
            }
            if (!FileExist(downloadToken.TargetFilePath))
            {
                return new HttpResponseDto() { HttpCode = 404, Message = "file not found" };
            }
            fileStream = new FileStream(downloadToken.TargetFilePath, FileMode.Open, FileAccess.Read);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.StackTrace);
            _logger.LogError(ex.Message);
            throw new HttpErrorException(new HttpResponseDto
            {
                HttpCode = 500,
                Message = "fail to get filestream",
            });
        }
    }
}