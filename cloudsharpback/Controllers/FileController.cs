using cloudsharpback.Controllers.Base;
using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : AuthControllerBase
    {
        private readonly IMemberFileService _memberFileService;
        private readonly ITicketStore _ticketStore;
        private readonly IShareService _shareService;
        private readonly ITusService _tusService;

        public FileController(IMemberFileService memberFileService, IShareService shareService, ITusService tusService, ITicketStore ticketStore)
        {
            this._memberFileService = memberFileService;
            this._shareService = shareService;
            this._tusService = tusService;
            _ticketStore = ticketStore;
        }

        [HttpGet("files")]
        public IActionResult GetFiles(string? path)
        {
            return Ok(_memberFileService.GetFiles(Member.Directory, path));
        }

        [ProducesResponseType(404)]
        [HttpGet("file")]
        public IActionResult GetFile(string path)
        {
            if (!_memberFileService.GetFile(Member, path, out var fileDto))
            {
                return NotFound();
            }

            return Ok(fileDto);
        }

        [HttpGet("dl_ticket")]
        public IActionResult GetDownloadToken(string path)
        {
            var err = _memberFileService.CheckBeforeTicketAdd(Member, path);
            if (err is not null)
            {
                return StatusCode(err!.ErrorCode, err.Message);
            }
            var dl = new DownloadToken(Member.Directory, path, DownloadType.Download);
            var ticket = new Ticket(HttpContext, TicketType.Download, dl);
            _ticketStore.Add(ticket);
            return Ok(ticket.Token.ToString());
        }

        [HttpGet("view_ticket")]
        public IActionResult GetViewToken(string path)
        {
            var err = _memberFileService.CheckBeforeTicketAdd(Member, path, true);
            if (err is not null)
            {
                return StatusCode(err!.ErrorCode, err.Message);
            }
            var dl = new DownloadToken(Member.Directory, path, DownloadType.View);
            var ticket = new Ticket(HttpContext, TicketType.ViewFile, dl);
            _ticketStore.Add(ticket);
            return Ok(ticket.Token.ToString());
        }

        [HttpGet("tusToken")]
        public IActionResult GetTusToken()
        {
            var err = _tusService.GetTusToken(Member, out var token);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok(token.ToString());
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(string path)
        {
            if (!_memberFileService.DeleteFile(Member, path, out var fileDto))
            {
                return StatusCode(404);
            }
            await _shareService.DeleteShareAsync(path, Member);
            return Ok(fileDto);
        }
    }
}
