using CliWrap;
using cloudsharpback.Hubs;
using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace cloudsharpback.Services
{
    public class YoutubeDlService : IYoutubeDlService
    {
        private readonly IHubContext<YoutubeDlHub> _hubContext;
        private string DirectoryPath;
        private readonly ILogger _logger;
        private readonly IJWTService jwtService;
        public YoutubeDlService(IHubContext<YoutubeDlHub> hubContext, IConfiguration configuration, ILogger<IYoutubeDlService> logger, IJWTService jwtService)
        {
            _hubContext = hubContext;
            YoutubeDlHub.OnConnect += OnSignalrConnected;
            DirectoryPath = configuration["File:DirectoryPath"];
            _logger = logger;
            this.jwtService = jwtService;
        }
        private readonly Dictionary<string, (string id, MemberDto member)> SignalrUsers = new();

        string userPath(string directoryId) => Path.Combine(DirectoryPath, directoryId);

        private async Task SendHubAuthError(string id, string detail)
            => await _hubContext.Clients.Client(id).SendAsync("AuthError", detail);

        private async Task SendHubConnected(string id, string detail)
            => await _hubContext.Clients.Client(id).SendAsync("Connected", detail);

        private async Task SendHubProgress(string id, Guid requsestToken, string progress)
            => await _hubContext.Clients.Client(id).SendAsync("DlProgress", requsestToken.ToString(), progress);

        private async Task SendHubDone(string id, Guid requsestToken)
            => await _hubContext.Clients.Client(id).SendAsync("DlDone", requsestToken.ToString());

        private async Task SendHubError(string id, Guid requsestToken, string detail)
            => await _hubContext.Clients.Client(id).SendAsync("DlError", requsestToken.ToString(), detail);

        private async Task DLYoutube(string youtubeUrl, string directory, string userid, Guid requsestToken)
        {
            await Cli.Wrap("yt-dlp")
            .WithArguments(youtubeUrl)
            .WithStandardOutputPipe(PipeTarget.ToDelegate((text) => SendHubProgress(userid, requsestToken, text)))
            .WithStandardErrorPipe(PipeTarget.ToDelegate((text) => SendHubError(userid, requsestToken, text)))
            .WithWorkingDirectory(directory)
            .ExecuteAsync();
            await SendHubDone(userid, requsestToken);
        }

        private async void OnSignalrConnected(HubCallerContext ctx)
        {
            var connId = ctx.ConnectionId;
            var httpctx = ctx.GetHttpContext();
            if (httpctx is null)
            {
                ctx.Abort();
                return;
            }
            if (!httpctx.Request.Headers.TryGetValue("auth", out var auth)
                || auth.FirstOrDefault() is null
                || !jwtService.TryValidateAcessToken(auth.First(), out var memberDto)
                || memberDto is null)
            {
                await SendHubAuthError(connId, "bad auth");
                ctx.Abort();
                return;
            }
            SignalrUsers.Add(auth.First(), (connId, memberDto));
            await SendHubConnected(connId, "connected");
        }

        public HttpErrorDto? Download(string auth, string youtubeUrl, string path, Guid requsestToken)
        {
            if (!SignalrUsers.TryGetValue(auth, out var conn))
            {
                return new HttpErrorDto() { ErrorCode = 404, Message = "connection not found" };
            }
            var dir = Path.Combine(userPath(conn.member.Directory), path ?? string.Empty);
            Task.Run(() => DLYoutube(youtubeUrl, dir, conn.id, requsestToken));
            return null;
        }
    }
}
