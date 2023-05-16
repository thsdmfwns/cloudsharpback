using Org.BouncyCastle.Asn1.X509;

namespace cloudsharpback.Models;

public class Ticket
{
    public Ticket(Guid token, string requestIpAddress, MemberDto? owner, DateTime expireTime, Type targetType, object target)
    {
        Token = token;
        RequestIpAddress = requestIpAddress;
        Owner = owner;
        ExpireTime = expireTime;
        TargetType = targetType;
        Target = target;
    }

    public Guid Token { get; }
    public DateTime ExpireTime { get;}
    public string RequestIpAddress { get; }
    public MemberDto? Owner { get; }
    public Object Target { get; }
    public Type TargetType { get; }

    public static DateTime GetExpireTime(TicketType type)
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