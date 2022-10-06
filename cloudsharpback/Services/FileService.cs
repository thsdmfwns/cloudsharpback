using JsonWebToken;
using System.IO;

namespace cloudsharpback.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger _logger;
        public string DirectoryPath { get; private set; }
        public FileService(IConfiguration configuration, ILogger<IFileService> logger)
        {
            DirectoryPath = configuration["File:DirectoryPath"];
            _logger = logger;
        }

        public bool TryMakeDirectory(string directoryId)
        {
            try
            {
                string path = DirectoryPath + directoryId;
                Directory.CreateDirectory(path);
                string subPath(string foldername) => Path.Combine(path, foldername);
                Directory.CreateDirectory(subPath("Download"));
                Directory.CreateDirectory(subPath("Music"));
                Directory.CreateDirectory(subPath("Video"));
                Directory.CreateDirectory(subPath("Document"));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                return false;
            }

        }
    }
}
