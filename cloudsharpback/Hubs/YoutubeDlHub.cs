using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace cloudsharpback.Hubs
{
    public class YoutubeDlHub : Hub
    {
        private readonly IYoutubeDlService _youtubeDlService;
        public YoutubeDlHub(IYoutubeDlService youtubeDlService)
        {
            this._youtubeDlService = youtubeDlService;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            var connId = Context.ConnectionId;
            var httpctx = Context.GetHttpContext();
            if (httpctx is null
                || !httpctx.Request.Query.TryGetValue("token", out var token)
                || token.FirstOrDefault() is null)
            {
                Context.Abort();
                return;
            }
            var authString = token.First();
            if (!await _youtubeDlService.ValidateConnectionToken(connId, authString))
            {
                Context.Abort();
                return;
            }
        }
    }
}
