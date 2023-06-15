namespace cloudsharpback.Models;

public class FileUploadRequestDto
{
    public required string FileName { get; set; }
    public string? UploadDirectory { get; set; }
}