using cloudsharpback.Services.Interfaces;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Stores;

namespace cloudsharpback.Services
{
    public class TusService : ITusService
    {
        private readonly ILogger _logger;
        private readonly IJWTService jWTService;
        private string TusStorePath;
        private string DirectoryPath;

        public TusService(ILogger<ITusService> logger, IConfiguration configuration, IJWTService jWTService)
        {
            _logger = logger;
            TusStorePath = configuration["File:TusStorePath"];
            DirectoryPath = configuration["File:DirectoryPath"];
            this.jWTService = jWTService;
        }

        public DefaultTusConfiguration GetTusConfiguration()
        {
            if (!Directory.Exists(TusStorePath)) Directory.CreateDirectory(TusStorePath);
            return new DefaultTusConfiguration
            {
                // This method is called on each request so different configurations can be returned per user, domain, path etc.
                // Return null to disable tusdotnet for the current request.

                // c:\tusfiles is where to store files
                Store = new TusDiskStore(TusStorePath),
                // On what url should we listen for uploads?
                UrlPath = "/api/tus",
                Events = new Events
                {
                    OnFileCompleteAsync = async eventContext =>
                    {
                        ITusFile file = await eventContext.GetFileAsync();
                        Dictionary<string, Metadata> metadata = await file.GetMetadataAsync(eventContext.CancellationToken);
                        using Stream content = await file.GetContentAsync(eventContext.CancellationToken);
                    }
                }
            };
        }
    }
}
