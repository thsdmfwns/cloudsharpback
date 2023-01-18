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
        private IPathStore _pathStore;

        public TusService(ILogger<ITusService> logger, IPathStore pathStore)
        {
            _logger = logger;
            _pathStore = pathStore;
        }

        private readonly Dictionary<string, string> _targets = new ();
        private string MemberDirectory(string directoryId) => _pathStore.MemberDirectory(directoryId);
        private string TusStorePath => _pathStore.TusStorePath;
        bool FileExist(string filePath) => System.IO.File.Exists(filePath);

        public HttpErrorDto? GetTusToken(MemberDto member, out Guid token)
        {
            token = Guid.NewGuid();
            if (!_targets.TryAdd(token.ToString(), member.Directory))
            {
                return new HttpErrorDto() { ErrorCode = 409, Message = "try again" };
            }
            return null;
        }

        public DefaultTusConfiguration GetTusConfiguration()
        {
            if (!Directory.Exists(TusStorePath)) Directory.CreateDirectory(TusStorePath);
            return new DefaultTusConfiguration
            {
                Store = new TusDiskStore(TusStorePath),
                UrlPath = "/api/tus",
                Events = new Events
                {
                    OnAuthorizeAsync = OnAuth, 
                    OnFileCompleteAsync = OnFileCompleteAsync,
                    OnBeforeCreateAsync = OnBeforeCreate,
                }
            };
        }

        Task OnAuth(AuthorizeContext context)
        {
            var request = context.HttpContext.Request;
            if (!request.Headers.TryGetValue("req_token", out var req_token))
            {
                context.FailRequest(System.Net.HttpStatusCode.Unauthorized, "bad req_token");
            }
            return Task.CompletedTask;
        }

        Task OnBeforeCreate(BeforeCreateContext context)
        {
            var request = context.HttpContext.Request;
            if (!request.Headers.TryGetValue("req_token", out var req_token))
            {
                context.FailRequest(System.Net.HttpStatusCode.Unauthorized, "req_token is required in request header");
                return Task.CompletedTask;
            }
            if (!_targets.TryGetValue(req_token, out var directoryId))
            {
                context.FailRequest(System.Net.HttpStatusCode.BadRequest, "bad req_token");
                return Task.CompletedTask;
            }
            if (!context.Metadata.ContainsKey("filepath")
                || !context.Metadata.ContainsKey("filename"))
            {
                context.FailRequest(System.Net.HttpStatusCode.BadRequest, "can not find required metadata");
                return Task.CompletedTask;
            }
            var metadata = context.Metadata;
            var directory = MemberDirectory(directoryId);
            var fileName = metadata["filename"].GetString(System.Text.Encoding.UTF8);
            var filepath = metadata["filepath"].GetString(System.Text.Encoding.UTF8);
            var target = Path.Combine(directory, filepath, fileName);
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
                var request = ctx.HttpContext.Request;
                if (!request.Headers.TryGetValue("req_token", out var req_token))
                {
                    throw new Exception("Can not find directoryId");
                }
                var dirId = _targets[req_token];
                var directory = MemberDirectory(dirId);
                ITusFile file = await ctx.GetFileAsync();
                Dictionary<string, Metadata> metadata = await file.GetMetadataAsync(ctx.CancellationToken);
                var fileName = metadata["filename"].GetString(System.Text.Encoding.UTF8);
                var filepath = metadata["filepath"].GetString(System.Text.Encoding.UTF8);
                var target = Path.Combine(directory, filepath, fileName);
                using (var targetStream = File.Create(target))
                using (Stream content = await file.GetContentAsync(ctx.CancellationToken))
                {
                    content.CopyTo(targetStream);
                }
                var terminationStore = (ITusTerminationStore)ctx.Store;
                await terminationStore.DeleteFileAsync(file.Id, ctx.CancellationToken);
                _targets.Remove(req_token);
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
