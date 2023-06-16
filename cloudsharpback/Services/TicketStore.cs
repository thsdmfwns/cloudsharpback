using System.Collections.Concurrent;
using cloudsharpback.Models;
using ITicketStore = cloudsharpback.Services.Interfaces.ITicketStore;

namespace cloudsharpback.Services;

public class TicketStore : ITicketStore
{
    private readonly ConcurrentDictionary<Guid, Ticket> _tickets = new();

    private void RemoveExpired() => _tickets
        .Where(x => x.Value.ExpireTime < DateTime.Now)
        .Select(x => x.Key)
        .ToList()
        .ForEach(x => _tickets.Remove(x, out _));

    public void Add(Ticket ticket)
        => _tickets.TryAdd(ticket.Token, ticket);

    public bool TryGet(Guid ticketToken, out Ticket? ticket)
    {
        RemoveExpired();
        ticket = null;
        if (!_tickets.TryGetValue(ticketToken, out var value)
            || value.ExpireTime < DateTime.Now)
        {
            return false;    
        }
        ticket = value;
        return true;
    }

    public void Remove(Guid ticketToken)
    {
        RemoveExpired();
        _tickets.Remove(ticketToken, out _);
    }
}