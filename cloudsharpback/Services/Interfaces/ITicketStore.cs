using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces;

public interface ITicketStore
{
    void Add(Ticket ticket);
    bool TryGet(Guid ticketToken, out Ticket? ticket);
}