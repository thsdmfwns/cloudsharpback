using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers;

[Route("dl")]
[ApiController]
public class DownloadController : ControllerBase
{
    private readonly ITicketStore _ticketStore;
    private readonly IFileStreamService _fileStreamService;

    public DownloadController(ITicketStore ticketStore, IFileStreamService fileStreamService)
    {
        _ticketStore = ticketStore;
        _fileStreamService = fileStreamService;
    }

    [HttpGet("{ticketToken}")]
    public IActionResult Download(string ticketToken)
    {
        if (!Guid.TryParse(ticketToken, out var ticketGuidToken))
        {
            return StatusCode(400, "bad token");
        };
        if (!_ticketStore.TryGet(ticketGuidToken, out var ticket)
            || ticket is null)
        {
            return StatusCode(404, "ticket not found");
        }
        var err = _fileStreamService.GetFileStream(ticket, out var fs);
        if (err is not null || fs is null)
        {
            return StatusCode(err!.ErrorCode, err.Message);
        }
        var res = new FileStreamResult(fs, MimeTypeUtil.GetMimeType(fs.Name)?? "application/octet-stream")
        {
            EnableRangeProcessing = true
        };
        if (((DownloadToken)ticket.Target!).DownloadType == DownloadType.Download)
        {
            res.FileDownloadName = Path.GetFileName(fs.Name);
        }
        _ticketStore.Remove(ticketGuidToken);
        return res;
    }
}