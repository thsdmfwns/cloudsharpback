using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces;

public interface IFileStreamService
{
    HttpErrorDto? GetFileStream(Ticket ticket, out FileStream? fileStream);
}