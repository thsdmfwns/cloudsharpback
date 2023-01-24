using cloudsharpback.Models;
using ITicketStore = cloudsharpback.Services.Interfaces.ITicketStore;

namespace cloudsharpback.Services;

public class TicketStore : ITicketStore
{
    private Dictionary<Guid, Ticket> _tickets = new Dictionary<Guid, Ticket>();

    private void RemoveExpired() => _tickets
        .Where(x => x.Value.ExpireTime < DateTime.Now)
        .Select(x => x.Key)
        .ToList()
        .ForEach(x => _tickets.Remove(x));

    public void Add(MemberDto memberDto, TicketType type, out Guid ticketToken, string? target = null)
    {
        ticketToken = new Guid();
        var value = new Ticket(ticketToken, memberDto, DateTime.Now.AddMinutes(3), type);
        if (target is not null)
        {
            value.Target = target;
        }
        _tickets.Add(ticketToken, value);
    }

    public bool TryGet(Guid ticketToken, out Ticket? ticket)
    {
        RemoveExpired();
        ticket = null;
        if (!_tickets.Remove(ticketToken, out var value))
        {
            return false;
        }
        if (value.Type is TicketType.ViewFile or TicketType.Signalr)
        {
            value.ExpireTime = DateTime.Now.AddDays(1);
            _tickets.Add(value.Value, value);
        }
        ticket = value;
        return true;
    }

    public bool TrySetTarget(Guid ticketToken, string target)
    {
        if (!_tickets.ContainsKey(ticketToken))
        {
            return false;
        }
        _tickets[ticketToken].Target = target;
        return true;
    }
}