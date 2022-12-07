using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace cloudsharpback.Hubs
{
    public class YoutubeDlHub : Hub
    {
        public static event Action<HubCallerContext>? OnConnect;

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            if (OnConnect is null)
            {
                Context.Abort();
                return;
            }
            OnConnect.Invoke(Context);
        }
    }
}
