using cloudsharpback.Models;
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
            if (!_ticketStore.TryGet(guidToken, out var ticket) ||
                ticket?.TicketType != TicketType.TusUpload || 
                ticket.Value is not FileUploadTicketValue uploadTarget)
            {
                context.FailRequest(System.Net.HttpStatusCode.BadRequest, "bad token");
                return Task.CompletedTask;
            }
            if (!Directory.Exists(uploadTarget.UploadDirectoryPath))
            {
                context.FailRequest(System.Net.HttpStatusCode.NotFound);
                _ticketStore.Remove(guidToken);
                return Task.CompletedTask;
            }
            var target = Path.Combine(uploadTarget.UploadDirectoryPath, uploadTarget.FileName);
            if (FileExist(target))
            {
                context.FailRequest(System.Net.HttpStatusCode.Conflict);
                _ticketStore.Remove(guidToken);
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }

        async Task OnFileCompleteAsync(FileCompleteContext ctx)
        {
            try
            {
                ITusFile file = await ctx.GetFileAsync();
                var terminationStore = (ITusTerminationStore)ctx.Store;
                var request = ctx.HttpContext.Request;
                if (!request.Headers.TryGetValue("token", out var token) ||
                    !Guid.TryParse(token, out var guidToken) ||
                    !_ticketStore.TryGet(guidToken, out var ticket) ||
                    ticket?.TicketType != TicketType.TusUpload || 
                    ticket.Value is not FileUploadTicketValue uploadTarget)
                {
                    await terminationStore.DeleteFileAsync(file.Id, ctx.CancellationToken);
                    throw new Exception("Can not find directoryId");
                }
                var target = Path.Combine(uploadTarget.UploadDirectoryPath, uploadTarget.FileName);
                using (var targetStream = File.Create(target))
                using (Stream content = await file.GetContentAsync(ctx.CancellationToken))
                {
                    content.CopyTo(targetStream);
                }
                await terminationStore.DeleteFileAsync(file.Id, ctx.CancellationToken);
                _ticketStore.Remove(guidToken);
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
