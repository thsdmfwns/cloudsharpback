using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.Ticket;

namespace cloudsharpback.Services.Interfaces;

public interface IFileStreamService
{
    HttpResponseDto? GetFileStream(DownloadTicket downloadToken, out FileStream? fileStream);
}