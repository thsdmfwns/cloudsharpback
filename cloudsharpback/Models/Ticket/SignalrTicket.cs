using cloudsharpback.Models.DTO.Member;
using Newtonsoft.Json;

namespace cloudsharpback.Models.Ticket;

public class SignalrTicket : ITicket<SignalrTicket>
{
    public TimeSpan? ExpireTime { get; init; } = TimeSpan.FromSeconds(30);
    public Guid Token { get; init; } = new Guid();
    public MemberDto? Owner { get; init; }
    public static SignalrTicket? FromJson(string? json)
    {
        return json is null 
            ? null 
            : JsonConvert.DeserializeObject<SignalrTicket>(json);
    }

    public static string RedisKey => "signalr";
}
