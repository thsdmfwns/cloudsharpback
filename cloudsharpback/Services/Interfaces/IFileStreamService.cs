using cloudsharpback.Models;
using cloudsharpback.Models.DTO;

namespace cloudsharpback.Services.Interfaces;

public interface IFileStreamService
{
    HttpResponseDto? GetFileStream(Ticket ticket, out FileStream? fileStream);
}