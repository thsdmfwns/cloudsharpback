namespace cloudsharpback.Services.Interfaces;

public interface IPathStore
{
    string DirectoryPath { get; }
    string TusStorePath { get; }
    string ProfilePath { get; }
    string MemberDirectory(string directoryId);
    string GetMemberTargetPath(string memberDirectoryId, string? targetPath);
}