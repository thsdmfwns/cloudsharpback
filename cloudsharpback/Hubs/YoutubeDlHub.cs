using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace cloudsharpback.Hubs
{
    public class YoutubeDlHub : Hub
    {
        public static event Action<string, string>? OnConnect;

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            var connId = Context.ConnectionId;
            var httpctx = Context.GetHttpContext();
            if (httpctx is null || OnConnect is null
                || !httpctx.Request.Headers.TryGetValue("auth", out var auth)
                || auth.FirstOrDefault() is null)
            {
                Context.Abort();
                return;
            }
            var authString = auth.First();
            OnConnect.Invoke(connId, authString);
        }
    }
}
