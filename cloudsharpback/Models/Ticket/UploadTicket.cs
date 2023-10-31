using cloudsharpback.Models.DTO.Member;
using Newtonsoft.Json;

namespace cloudsharpback.Models.Ticket;

public class UploadTicket : ITicket<UploadTicket>
{
    public required string UploadDirectoryPath { get; init; }
    public required string FileName { get; init; }
    public TimeSpan? ExpireTime { get; init; }
    public Guid Token { get; init; } = Guid.NewGuid();
    public MemberDto? Owner { get; init; }

    public static UploadTicket? FromJson(string? json)
    {
        return json is null 
            ? null 
            : JsonConvert.DeserializeObject<UploadTicket>(json);
    }
    public static string RedisKey => "upload";
}