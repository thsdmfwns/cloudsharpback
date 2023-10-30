using cloudsharpback.Models;
using cloudsharpback.Models.Ticket;

namespace cloudsharpback.Services.Interfaces;

public interface ITicketStore
{
    ValueTask<bool> TryAdd(string key, Guid Token, object obj, TimeSpan? timeSpan = null);
    ValueTask<bool> TryAddDownloadTicketAsync(DownloadTicket downloadTicket);
    ValueTask<bool> TryAddUpLoadTicketAsync(UploadTicket uploadTicket);
    ValueTask<string?> GetAsync(string key, Guid ticketToken);
    ValueTask<UploadTicket?> GetUploadTicket(Guid guidToken);
    ValueTask<DownloadTicket?> GetDownloadTicket(Guid guidToken);
    public ValueTask<bool> Remove(string key, Guid ticketToken);
    ValueTask<bool> RemoveDownloadTicket(DownloadTicket downloadTicket);
    ValueTask<bool> RemoveUploadTicket(UploadTicket uploadTicket);

}