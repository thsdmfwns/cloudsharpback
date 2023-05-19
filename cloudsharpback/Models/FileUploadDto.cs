namespace cloudsharpback.Models;

public class FileUploadDto
{
    public FileUploadDto(string fileName, string? filePath)
    {
        FileName = fileName;
        FilePath = filePath;
    }

    public string FileName { get; }
    public string? FilePath { get; }
}