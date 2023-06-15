namespace cloudsharpback.Models;

public class FileDownloadTicketValue
{
    public required string TargetFilePath { get; set; }
    public required FileDownloadType FileDownloadType { get; set; }
}

