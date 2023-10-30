using Newtonsoft.Json;

namespace cloudsharpback.Models.Ticket;

public class UploadTicket : TicketBase
{
    public required string UploadDirectoryPath { get; set; }
    public required string FileName { get; set; }
    
    public static UploadTicket? FromJson(string? json) => JsonConvert.DeserializeObject<UploadTicket>(json);

    public const string RedisKey = "upload";
}