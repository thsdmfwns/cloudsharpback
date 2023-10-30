using cloudsharpback.Models.DTO.Member;

namespace cloudsharpback.Models.Ticket;

public abstract class TicketBase
{
    public Guid Token { get; init; } = Guid.NewGuid();
    public MemberDto? Owner { get; init; }
}