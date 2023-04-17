using Org.BouncyCastle.Asn1.X509;

namespace cloudsharpback.Models;

public class Ticket
{
    public Ticket(string directory, TicketType type, string? requestIpAddress, MemberDto? member, string? target = null)
    {
        Directory = directory;
        ExpireTime = GetExpireTime(type);
        Type = type;
        RequestIpAddress = requestIpAddress;
        Member = member;
        Token = Guid.NewGuid();
        Target = target;
    }

    public Guid Token { get; }
    public DateTime ExpireTime { get; set; }
    public string Directory { get; }
    public TicketType Type { get; }
    public string? Target { get; set; }
    public string? RequestIpAddress { get; }
    public MemberDto? Member { get; }

    private DateTime GetExpireTime(TicketType type)
    {
        return type switch
        {
            TicketType.Download => DateTime.Now.AddMinutes(10),
            TicketType.ViewFile => DateTime.Now.AddDays(1),
            TicketType.Signalr => DateTime.Now.AddMinutes(10),
            _ => DateTime.Now.AddMinutes(10)
        };
    }
}