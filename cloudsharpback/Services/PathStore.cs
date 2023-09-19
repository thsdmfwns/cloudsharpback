using cloudsharpback.Services.Interfaces;

namespace cloudsharpback.Services;

public class PathStore : IPathStore
{
    private readonly string _volumePath;
    public PathStore(IEnvironmentValueStore environmentValueStore)
    {
        _volumePath = environmentValueStore[RequiredEnvironmentValueKey.CS_VOLUME_PATH];
    }

    private string CreateDirIfNotExist(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }
    
    public string DirectoryPath => CreateDirIfNotExist(Path.Combine(_volumePath, "Directory"));
    public string TusStorePath => CreateDirIfNotExist(Path.Combine(_volumePath, "TusStore"));
    public string ProfilePath => CreateDirIfNotExist(Path.Combine(_volumePath, "Profile"));
    
    public string MemberDirectory(string directoryId)
    {
        if (!Guid.TryParse(directoryId, out _))
        {
            throw new IOException("DirectoryId is not guid");
        }
        var dir = new DirectoryInfo(Path.Combine(DirectoryPath, directoryId));
        if (dir.Exists) return dir.FullName;
        
        Directory.CreateDirectory(dir.FullName);
        return dir.FullName;
    }
    
    public string GetMemberTargetPath(string memberDirectoryId, string? targetPath)
    {
        var memberDirectory = MemberDirectory(memberDirectoryId);
        targetPath ??= string.Empty;
        if (Path.IsPathRooted(targetPath))
        {
            throw new IOException("Target path is rooted");
        }
        return Path.Combine(memberDirectory, targetPath);
    }
    
}