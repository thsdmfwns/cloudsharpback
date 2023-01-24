using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces;

public interface ITicketStore
{
    void Add(MemberDto memberDto, TicketType type, out Guid ticketToken, string? tartget = null);
    bool TryGet(Guid ticketToken, out Ticket? ticket);
    bool TrySetTarget(Guid ticketToken, string target);
}