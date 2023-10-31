using cloudsharpback.Models.DTO.Member;
using Newtonsoft.Json;

namespace cloudsharpback.Models.Ticket;

public class DownloadTicket : ITicket<DownloadTicket>
{
    public required string TargetFilePath { get; init; }
    public required FileDownloadType FileDownloadType { get; init; }
    public TimeSpan? ExpireTime { get; init; }
    public Guid Token { get; init; }
    public MemberDto? Owner { get; init; }
    public static DownloadTicket? FromJson(string? json)
    {
        return json is null 
            ? null 
            : JsonConvert.DeserializeObject<DownloadTicket>(json);
    }
    public static string RedisKey => "download";

}

public enum FileDownloadType
{
    Download,
    View,
}

