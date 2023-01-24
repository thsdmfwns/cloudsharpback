﻿using cloudsharpback.Services.Interfaces;
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
                || !httpctx.Request.Query.TryGetValue("auth", out var auth)
                || auth.FirstOrDefault() is null)
            {
                Context.Abort();
                return;
            }
            var authString = auth.First();
            await _youtubeDlService.OnSignalrConnected(connId, authString);
        }
    }
}
