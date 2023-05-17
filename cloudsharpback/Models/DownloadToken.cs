namespace cloudsharpback.Models;

public class DownloadToken
{
    public DownloadToken(string fileDirectory, string fIlePath, DownloadType downloadType)
    {
        FileDirectory = fileDirectory;
        FIlePath = fIlePath;
        DownloadType = downloadType;
    }

    public string FileDirectory { get; }
    public string FIlePath { get; }
    public DownloadType DownloadType { get; }
    
}

