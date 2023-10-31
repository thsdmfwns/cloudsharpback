using cloudsharpback.Models;
using cloudsharpback.Models.Ticket;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utils;
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
    public async Task<IActionResult> Download(string ticketToken)
    {
        if (!Guid.TryParse(ticketToken, out var guidToken))
        {
            return StatusCode(400, "bad token");
        }

        var ticket = await _ticketStore.GetTicket<DownloadTicket>(guidToken);
        if (ticket is null)
        {
            return StatusCode(404, "ticket not found");
        }
        var err = _fileStreamService.GetFileStream(ticket, out var fs);
        if (err is not null || fs is null)
        {
            return StatusCode(err!.HttpCode, err.Message);
        }
        var res = new FileStreamResult(fs, MimeTypeUtil.GetMimeType(fs.Name)?? "application/octet-stream")
        {
            EnableRangeProcessing = true
        };
        if (((DownloadTicket)ticket).FileDownloadType == FileDownloadType.Download)
        {
            res.FileDownloadName = Path.GetFileName(fs.Name);
        }
        await _ticketStore.RemoveTicket(ticket);
        return res;
    }
}