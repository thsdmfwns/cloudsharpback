using cloudsharpback.Models.Ticket;

namespace cloudsharpback.Repository.Interface;

public interface ITicketStore
{
    public ValueTask<bool> AddTicket<T>(ITicket<T> ticket) where T : ITicket<T>;
    public ValueTask<T?> GetTicket<T>(Guid ticketToken) where T : ITicket<T>;
    public ValueTask<bool> RemoveTicket<T>(ITicket<T> ticket) where T : ITicket<T>;
    public ValueTask<bool> ExistTicket<T>(Guid ticketToken) where T : ITicket<T>;
    public ValueTask<bool> SetTicketExpire<T>(Guid ticketToken, TimeSpan timeSpan) where T : ITicket<T>;


}