using cloudsharpback.Utills;
using Org.BouncyCastle.Asn1.X509;

namespace cloudsharpback.Models;

public class Ticket
{
    public Ticket(string? requestIpAddress, MemberDto? owner, TicketType ticketType, object value)
    {
        Token = Guid.NewGuid();
        RequestIpAddress = requestIpAddress ?? string.Empty;
        Owner = owner;
        ExpireTime = DateTime.Now.AddMinutes(10);
        TicketType = ticketType;
        Value = value;
    }
    
    public Ticket(HttpContext httpContext, TicketType ticketType, object? value)
    {
        Token = Guid.NewGuid();
        RequestIpAddress = IpAdressUtil.Get(httpContext) ?? string.Empty;
        Owner = httpContext.Items["member"] as MemberDto;
        ExpireTime = DateTime.Now.AddMinutes(10);
        TicketType = ticketType;
        Value = value;
    }
    
    public Ticket(HttpContext httpContext, DateTime expireTime, TicketType ticketType, object? value)
    {
        Token = Guid.NewGuid();
        RequestIpAddress = IpAdressUtil.Get(httpContext) ?? string.Empty;
        Owner = httpContext.Items["member"] as MemberDto;
        ExpireTime = expireTime;
        TicketType = ticketType;
        Value = value;
    }

    public Guid Token { get; }
    public DateTime ExpireTime { get;}
    public string RequestIpAddress { get; }
    public MemberDto? Owner { get; }
    public Object? Value { get; }
    public TicketType TicketType { get; }

    /*public static DateTime GetExpireTime(TicketType type)
    {
        return type switch
        {
            TicketType.Download => DateTime.Now.AddMinutes(10),
            TicketType.ViewFile => DateTime.Now.AddDays(1),
            TicketType.Signalr => DateTime.Now.AddMinutes(10),
            _ => DateTime.Now.AddMinutes(10)
        };
    }*/
}