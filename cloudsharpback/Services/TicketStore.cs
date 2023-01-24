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

    public void Add(MemberDto memberDto, TicketType type, out Guid ticket)
    {
        ticket = new Guid();
        _tickets.Add(ticket, new Ticket(ticket, memberDto, DateTime.Now.AddMinutes(3), type));
    }

    public bool TryGet(Guid key, out Ticket? ticket)
    {
        RemoveExpired();
        ticket = null;
        if (!_tickets.Remove(key, out var value))
        {
            return false;
        }
        
        if (value.Type == TicketType.ViewFile)
        {
            value.ExpireTime = DateTime.Now.AddDays(1);
            _tickets.Add(value.Value, value);
        }
        
        ticket = value;
        return true;
    }
}