using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace cloudsharpback.Hubs
{
    public class YoutubeDlHub : Hub
    {
        private readonly IYoutubeDlService youtubeDlService;
        public YoutubeDlHub(IYoutubeDlService youtubeDlService)
        {
            this.youtubeDlService = youtubeDlService;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            var connId = Context.ConnectionId;
            var httpctx = Context.GetHttpContext();
            if (httpctx is null
                || !httpctx.Request.Query.TryGetValue("auth", out var auth)
                || auth.FirstOrDefault() is null)
            {
                Context.Abort();
                return;
            }
            var authString = auth.First();
            await youtubeDlService.OnSignalrConnected(connId, authString);
        }
    }
}
