using cloudsharpback.Models.Ticket;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Tusd.Protos;
using Grpc.Core;

namespace cloudsharpback.Services;

public class TusdHookService : Tusd.Protos.HookHandler.HookHandlerBase
{
    private readonly ILogger _logger;
    private readonly IPathStore _pathStore;
    private readonly ITicketStore _ticketStore;
    private readonly string _tusStorePath;

    public TusdHookService(ILogger<TusdHookService> logger, IPathStore pathStore, ITicketStore ticketStore)
    {
        _logger = logger;
        _pathStore = pathStore;
        _ticketStore = ticketStore;
        _tusStorePath = _pathStore.TusStorePath;
    }   

    public override async Task<HookResponse> InvokeHook(HookRequest request, ServerCallContext context)
    {
        var type = request.Type;
        if (type is null)
        {
            return GetResponse("not found tusd type", 500);
        }
        return type switch
        {
            "pre-create" => await OnBeforeCreate(request.Event),
            "pre-finish" => await OnBeforeFinish(request.Event),
            "post-finish" => await OnAfterFinish(request.Event),
            "post-terminate" => await OnTerminated(request.Event),
            _ => GetResponse(string.Empty, 200, isFail: false)
        };
    }
    private async ValueTask<HookResponse> OnBeforeCreate(Event hookEvent)
    {
        // validate ticket
        var metadata = GetMetaData(hookEvent);
        var ticket = await GetTicket(metadata);
        if (ticket is null)
        {
            return GetResponse("can't find ticket", 403);
        }
        var overwrite = GetOverwrite(metadata);
        // validate file exist
        var fileInfo = new System.IO.FileInfo(GetTargetPath(ticket));
        if (!overwrite && fileInfo.Exists)
        {
            return GetResponse("there is same name of file", 409);
        }
        
        if (!await _ticketStore.SetTicketExpire<UploadTicket>(ticket.Token, TimeSpan.FromDays(1)))
        {
            return GetResponse("not found ticket", 404);
        }
        return GetResponse(string.Empty, 200, false);
    }

    private async ValueTask<HookResponse> OnBeforeFinish(Event hookEvent)
    {
        // validate ticket
        var metadata = GetMetaData(hookEvent);
        var ticket = await GetTicket(metadata);
        if (ticket is null)
        {
            return GetResponse("can't find ticket", 403);
        }
        var overwrite = GetOverwrite(metadata);
        var targetInfo = new System.IO.FileInfo(GetTargetPath(ticket));
        if (!overwrite && targetInfo.Exists)
        {
            return GetResponse("there is same name of file", 409);
        }
        var uploadedFileInfo = new System.IO.FileInfo(Path.Combine(_pathStore.TusStorePath, hookEvent.Upload.Id));
        if (!uploadedFileInfo.Exists)
        {
            return GetResponse("not found uploaded file", 404);
        }
        uploadedFileInfo.MoveTo(targetInfo.FullName, overwrite);
        return GetResponse(string.Empty, 200, false);
    }

    private async ValueTask<HookResponse> OnAfterFinish(Event hookEvent)
    {
        var metadata = GetMetaData(hookEvent);
        var ticket = await _ticketStore.GetTicket<UploadTicket>(TryGetTicketToken(metadata).GetValueOrDefault());
        if (ticket is null)
        {
            return GetResponse("not found ticket", 404);
        }
        await _ticketStore.RemoveTicket(ticket);
        return GetResponse(string.Empty, 200, false);
    }

    private async ValueTask<HookResponse> OnTerminated(Event hookEvent)
    {
        var metadata = GetMetaData(hookEvent);
        var ticket = await _ticketStore.GetTicket<UploadTicket>(TryGetTicketToken(metadata).GetValueOrDefault());
        if (ticket is null)
        {
            return GetResponse("not found ticket", 404);
        }
        await _ticketStore.RemoveTicket(ticket);
        return GetResponse(string.Empty, 200, false);
    }
    
    private async Task<UploadTicket?> GetTicket(Dictionary<string, string> metadata)
    {
        var ticket = await _ticketStore.GetTicket<UploadTicket>(TryGetTicketToken(metadata).GetValueOrDefault());
        if (ticket?.Owner is null)
        {
            return null;
        }
        var dir = new DirectoryInfo(
            Path.Combine(_pathStore.MemberDirectory(ticket.Owner.Directory), ticket.UploadDirectoryPath));
        return !dir.Exists ? null : ticket;
    }
    
    private string GetTargetPath(UploadTicket ticket)
     => Path.Combine(_pathStore.GetMemberTargetPath(ticket.Owner!.Directory, ticket.UploadDirectoryPath), ticket.FileName);
    
    private Dictionary<string, string> GetMetaData(Event hookEvent)
        => hookEvent.Upload.MetaData.ToDictionary(x => x.Key, x => x.Value);

    private Guid? TryGetTicketToken(Dictionary<string, string> metadata)
    {
        if (!metadata.TryGetValue("token", out var tokenStr)
            || !Guid.TryParse(tokenStr, out var result))
        {
            return null;
        }
        return result;
    }

    private bool GetOverwrite(Dictionary<string, string> metadata)
    {
        if (!metadata.TryGetValue("overwrite", out var boolStr)
            || !bool.TryParse(boolStr, out var result))
        {
            return false;
        }
        return result;
    }
    
    private HookResponse GetResponse(string body, long code, bool isFail = true)
    {
        return new HookResponse()
        {
            HttpResponse = new HTTPResponse()
            {
                StatusCode = code,
                Body = body
            },
            RejectUpload = isFail,
            StopUpload = isFail
        };
    }
}