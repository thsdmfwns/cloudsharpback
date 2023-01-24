namespace cloudsharpback.Models;

public class Ticket
{
    public Ticket(Guid value, MemberDto member, DateTime expireTime, TicketType type)
    {
        Member = member;
        ExpireTime = expireTime;
        Type = type;
        Value = value;
    }

    public Guid Value { get; private set; }
    public DateTime ExpireTime { get; set; }
    public MemberDto Member { get; private set; }
    public TicketType Type { get; private set; }
    public string? Target { get; set; }
}