using cloudsharpback.Models.DTO.Member;
using Newtonsoft.Json;

namespace cloudsharpback.Models.Ticket;

public interface ITicket<T> where T : ITicket<T>
{
    public TimeSpan? ExpireTime { get; init; }
    public Guid Token { get; init; } 
    public MemberDto? Owner { get; init; }
    public static abstract T? FromJson(string? json) ;
    public static abstract string RedisKey { get; }
}