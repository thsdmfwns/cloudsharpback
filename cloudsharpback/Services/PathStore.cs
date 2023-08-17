using cloudsharpback.Services.Interfaces;

namespace cloudsharpback.Services;

public class PathStore : IPathStore
{
    private readonly string _volumePath;
    public PathStore(IEnvironmentValueStore environmentValueStore)
    {
        _volumePath = environmentValueStore[RequiredEnvironmentValueKey.CS_VOLUME_PATH];
        CreateDirIfNotExist(DirectoryPath);
        CreateDirIfNotExist(TusStorePath);
        CreateDirIfNotExist(ProfilePath);
    }

    private void CreateDirIfNotExist(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
    
    public string DirectoryPath => Path.Combine(_volumePath, "Directory");
    public string TusStorePath => Path.Combine(_volumePath, "TusStore");
    public string ProfilePath => Path.Combine(_volumePath, "Profile");
    public string MemberDirectory(string memberDirectoryId) =>
        Path.Combine(DirectoryPath, memberDirectoryId);
    
}