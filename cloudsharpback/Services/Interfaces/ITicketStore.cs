using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces;

public interface ITicketStore
{
    void Add(MemberDto memberDto, TicketType type, out Guid ticket, string? tartget = null);
    bool TryGet(Guid key, out Ticket? ticket);
}