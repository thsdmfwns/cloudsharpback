namespace cloudsharpback.Models.DTO.FIle;

public class FileUploadRequestDto
{
    public required string FileName { get; set; }
    public string? UploadDirectory { get; set; }
}