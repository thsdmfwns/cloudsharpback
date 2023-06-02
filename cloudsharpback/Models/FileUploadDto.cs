namespace cloudsharpback.Models;

public class FileUploadDto
{
    public FileUploadDto(string fileName, string? uploadDirectory)
    {
        FileName = fileName;
        UploadDirectory = uploadDirectory;
    }

    public string FileName { get; }
    public string? UploadDirectory { get; }
}