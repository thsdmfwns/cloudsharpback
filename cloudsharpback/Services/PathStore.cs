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
    public string MemberDirectory(string memberDirectoryId) =>
        Path.Combine(DirectoryPath, memberDirectoryId);
    
}