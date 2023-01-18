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
        private readonly IPathStore _pathStore;
        private readonly ILogger _logger;
        private readonly IJWTService _jwtService;
        public YoutubeDlService(IHubContext<YoutubeDlHub> hubContext, IPathStore pathStore, ILogger<IYoutubeDlService> logger, IJWTService jwtService)
        {
            _hubContext = hubContext;
            _pathStore = pathStore;
            _logger = logger;
            _jwtService = jwtService;
        }
        private readonly Dictionary<ulong, (string signalrUserId, MemberDto member)> _signalrUsers = new();

        private string MemberDirectory(string directoryId) => _pathStore.MemberDirectory(directoryId);

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

        private async Task DlYoutube(string youtubeUrl, string directory, string userid, Guid requestToken)
        {
            await Cli.Wrap("yt-dlp")
            .WithArguments(youtubeUrl)
            .WithStandardOutputPipe(PipeTarget.ToDelegate((text) => SendHubProgress(userid, requestToken, text)))
            .WithStandardErrorPipe(PipeTarget.ToDelegate((text) => SendHubError(userid, requestToken, text)))
            .WithWorkingDirectory(directory)
            .ExecuteAsync();
            await SendHubDone(userid, requestToken);
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

        public HttpErrorDto? Download(MemberDto member, string youtubeUrl, string? path, Guid requestToken)
        {
            try
            {
                if (!_signalrUsers.TryGetValue(member.Id, out var conn))
                {
                    return new HttpErrorDto() { ErrorCode = 404, Message = "connection not found" };
                }
                var dir = Path.Combine(MemberDirectory(conn.member.Directory), path ?? string.Empty);
                Task.Run(() => DlYoutube(youtubeUrl, dir, conn.signalrUserId, requestToken));
                return null;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.StackTrace);
                _logger.LogError(exception.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to Download Youtube",
                });
            }
        }
    }
}
