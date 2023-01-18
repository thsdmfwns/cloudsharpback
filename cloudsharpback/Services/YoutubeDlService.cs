using CliWrap;
using cloudsharpback.Hubs;
using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace cloudsharpback.Services
{
    public class YoutubeDlService : IYoutubeDlService
    {
        private readonly IHubContext<YoutubeDlHub> _hubContext;
        private readonly string _directoryPath;
        private readonly ILogger _logger;
        private readonly IJWTService _jwtService;
        public YoutubeDlService(IHubContext<YoutubeDlHub> hubContext, IConfiguration configuration, ILogger<IYoutubeDlService> logger, IJWTService jwtService)
        {
            _hubContext = hubContext;
            _directoryPath = configuration["File:DirectoryPath"];
            _logger = logger;
            this._jwtService = jwtService;
        }
        private readonly Dictionary<ulong, (string signalrUserId, MemberDto member)> _signalrUsers = new();

        string userPath(string directoryId) => Path.Combine(_directoryPath, directoryId);

        private async Task SendHubAuthError(string id, string detail)
            => await _hubContext.Clients.Client(id).SendAsync("AuthError", detail);

        private async Task SendHubConnected(string id, string detail)
            => await _hubContext.Clients.Client(id).SendAsync("Connected", detail);

        private async Task SendHubProgress(string id, Guid requestToken, string progress)
            => await _hubContext.Clients.Client(id).SendAsync("DlProgress", requestToken.ToString(), progress);

        private async Task SendHubDone(string id, Guid requestToken)
            => await _hubContext.Clients.Client(id).SendAsync("DlDone", requestToken.ToString());

        private async Task SendHubError(string id, Guid requestToken, string detail)
            => await _hubContext.Clients.Client(id).SendAsync("DlError", requestToken.ToString(), detail);

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

        public async Task OnSignalrConnected(string connId, string auth)
        {
            if (!_jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                await SendHubAuthError(connId, "bad auth");
                return;
            }
            _signalrUsers.Add(memberDto.Id, (connId, memberDto));
            await SendHubConnected(connId, "connected");
        }

        public HttpErrorDto? Download(MemberDto member, string youtubeUrl, string path, Guid requestToken)
        {
            if (!_signalrUsers.TryGetValue(member.Id, out var conn))
            {
                return new HttpErrorDto() { ErrorCode = 404, Message = "connection not found" };
            }
            var dir = Path.Combine(userPath(conn.member.Directory), path ?? string.Empty);
            Task.Run(() => DLYoutube(youtubeUrl, dir, conn.signalrUserId, requestToken));
            return null;
        }
    }
}
