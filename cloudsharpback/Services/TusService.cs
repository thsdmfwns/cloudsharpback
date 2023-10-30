using cloudsharpback.Models;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Stores;

//todo this service will replace with tusd
namespace cloudsharpback.Services
{
    public class TusService : ITusService
    {
        private readonly ILogger _logger;
        private readonly IPathStore _pathStore;
        private readonly ITicketStore _ticketStore;
        private readonly string _tusStorePath;

        public TusService(ILogger<ITusService> logger, IPathStore pathStore, ITicketStore ticketStore)
        {
            _logger = logger;
            _pathStore = pathStore;
            _ticketStore = ticketStore;
            _tusStorePath = _pathStore.TusStorePath;
        }
        
        private string MemberDirectory(string directoryId) => _pathStore.MemberDirectory(directoryId);
        bool FileExist(string filePath) => File.Exists(filePath);

        public DefaultTusConfiguration GetTusConfiguration()
        {
            if (!Directory.Exists(_tusStorePath)) Directory.CreateDirectory(_tusStorePath);
            return new DefaultTusConfiguration
            {
                Store = new TusDiskStore(_tusStorePath),
                UrlPath = "/api/upload",
                Events = new Events
                {
                    OnFileCompleteAsync = OnFileCompleteAsync,
                    OnBeforeCreateAsync = OnBeforeCreate,
                }
            };
        }

        Task OnBeforeCreate(BeforeCreateContext context)
        {
            var request = context.HttpContext.Request;
            if (!request.Headers.TryGetValue("token", out var token) ||
                !Guid.TryParse(token, out var guidToken))
            {
                context.FailRequest(System.Net.HttpStatusCode.Unauthorized, "token is required in header");
                return Task.CompletedTask;
            }
            // todo validate ticket
            // todo validate directory exist
            // todo validate file exist
            return Task.CompletedTask;
        }

        async Task OnFileCompleteAsync(FileCompleteContext ctx)
        {
            try
            {
                ITusFile file = await ctx.GetFileAsync();
                var terminationStore = (ITusTerminationStore)ctx.Store;
                var request = ctx.HttpContext.Request;
                // todo validate ticket
                // todo validate directory exist
                // todo copy to target
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw;
            }

        }
    }
}
