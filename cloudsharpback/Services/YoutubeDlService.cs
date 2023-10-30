using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CliWrap;
using cloudsharpback.Hubs;
using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace cloudsharpback.Services;

/// <summary>
///  TODO Refactor this
/// </summary>
public class YoutubeDlService : IYoutubeDlService
{
    private readonly IHubContext<YoutubeDlHub> _hubContext;
    private readonly IPathStore _pathStore;
    private readonly ILogger _logger;
    private readonly ITicketStore _ticketStore;
    public YoutubeDlService(IHubContext<YoutubeDlHub> hubContext, IPathStore pathStore, ILogger<IYoutubeDlService> logger, ITicketStore ticketStore)
    {
        _hubContext = hubContext;
        _pathStore = pathStore;
        _logger = logger;
        _ticketStore = ticketStore;
    }
    private readonly Dictionary<ulong, (string signalrUserId, MemberDto member)> _signalrUsers = new();

    private string MemberDirectory(string directoryId) => _pathStore.MemberDirectory(directoryId);

    private async Task SendHubConnectionResult(string id, string detail)
        => await _hubContext.Clients.Client(id).SendAsync("ConnectionResult", detail);

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

    public async Task<bool> ValidateConnectionToken(string connId, string tokenString)
    {
        /*if (!Guid.TryParse(tokenString, out var token) 
            || !_ticketStore.GetAsync(token, out var ticket))
        {
            var err = new HttpResponseDto()
            {
                HttpCode = 404,
                Message = "Token Not Found"
            };
            await SendHubConnectionResult(connId, JsonConvert.SerializeObject(err));
            return false;   
        }
        if (ticket?.Owner is null)
        {
            var err = new HttpResponseDto()
            {
                HttpCode = 400,
                Message = "Bad token"
            };
            await SendHubConnectionResult(connId, JsonConvert.SerializeObject(err));
            return false;
        }
        _signalrUsers.Add(ticket.Owner.Id, (connId, ticket.Owner));
        var res = new HttpResponseDto()
        {
            HttpCode = 200,
            Message = "Connected"
        };
        await SendHubConnectionResult(connId, JsonConvert.SerializeObject(res));
        return true;*/
        throw new ArgumentNullException();
    }

    public HttpResponseDto? Download(MemberDto member, string youtubeUrl, string? path, Guid requestToken)
    {
        try
        {
            if (!Regex.IsMatch(youtubeUrl, @"^(?:https?://)?(?:www\.)?(?:youtube\.com/watch\?v=|youtu\.be/)([\w-]+)(?:&\S*)?$"))
            {
                return new HttpResponseDto() { HttpCode = 400, Message = "Bad Youtube URL" };
            }
            if (!_signalrUsers.TryGetValue(member.Id, out var conn))
            {
                return new HttpResponseDto() { HttpCode = 401, Message = "connection not found" };
            }
            var dir = Path.Combine(MemberDirectory(conn.member.Directory), path ?? string.Empty);
            if (!Directory.Exists(dir))
            {
                return new HttpResponseDto() { HttpCode = 404, Message = "Directory not found" };
            }
            Task.Run(() => DlYoutube(youtubeUrl, dir, conn.signalrUserId, requestToken));
            return null;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.StackTrace);
            _logger.LogError(exception.Message);
            throw new HttpErrorException(new HttpResponseDto
            {
                HttpCode = 500,
                Message = "fail to Download Youtube",
            });
        }
    }
}