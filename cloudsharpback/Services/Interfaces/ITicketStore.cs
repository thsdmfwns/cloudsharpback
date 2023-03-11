using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces;

public interface ITicketStore
{
    void Add(Ticket ticket);
    bool TryGetAndRemove(Guid ticketToken, out Ticket? ticket);
    bool TryGet(Guid ticketToken, out Ticket? ticket);
    bool TrySetTarget(Guid ticketToken, string target);
}