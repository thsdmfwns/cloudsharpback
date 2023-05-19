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

        public TusService(ILogger<ITusService> logger, IPathStore pathStore, ITicketStore ticketStore)
        {
            _logger = logger;
            _pathStore = pathStore;
            _ticketStore = ticketStore;
        }
        
        private string MemberDirectory(string directoryId) => _pathStore.MemberDirectory(directoryId);
        private string TusStorePath => _pathStore.TusStorePath;
        bool FileExist(string filePath) => System.IO.File.Exists(filePath);

        public DefaultTusConfiguration GetTusConfiguration()
        {
            if (!Directory.Exists(TusStorePath)) Directory.CreateDirectory(TusStorePath);
            return new DefaultTusConfiguration
            {
                Store = new TusDiskStore(TusStorePath),
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
                ticket.Target is not TusUploadToken uploadTarget)
            {
                context.FailRequest(System.Net.HttpStatusCode.BadRequest, "bad token");
                return Task.CompletedTask;
            }
            var directory = MemberDirectory(uploadTarget.FileDirectory);
            var target = Path.Combine(directory, uploadTarget.FilePath, uploadTarget.FileName);
            if (FileExist(target))
            {
                context.FailRequest(System.Net.HttpStatusCode.Conflict);
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
                    ticket.Target is not TusUploadToken uploadTarget)
                {
                    await terminationStore.DeleteFileAsync(file.Id, ctx.CancellationToken);
                    throw new Exception("Can not find directoryId");
                }
                var target = Path.Combine(MemberDirectory(uploadTarget.FileDirectory), uploadTarget.FilePath, uploadTarget.FileName);
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
