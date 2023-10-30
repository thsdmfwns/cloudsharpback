using Newtonsoft.Json;

namespace cloudsharpback.Models.Ticket;

public class DownloadTicket : TicketBase
{
    public required string TargetFilePath { get; init; }
    public required FileDownloadType FileDownloadType { get; init; }

    public static DownloadTicket? FromJson(string? json) => JsonConvert.DeserializeObject<DownloadTicket>(json);

    public const string RedisKey = "download";
}

public enum FileDownloadType
{
    Download,
    View,
}

