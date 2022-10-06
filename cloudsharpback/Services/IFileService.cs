namespace cloudsharpback.Services
{
    public interface IFileService
    {
        string DirectoryPath { get; }

        bool TryMakeDirectory(string directoryId);
    }
}