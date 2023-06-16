using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces;

public interface IFileStreamService
{
    HttpResponseDto? GetFileStream(Ticket ticket, out FileStream? fileStream);
}