namespace cloudsharpback.Services.Interfaces;

public interface IPathStore
{
    string DirectoryPath { get; }
    string TusStorePath { get; }
    string ProfilePath { get; }
    string MemberDirectoryPath(string memberDirectoryId);
}