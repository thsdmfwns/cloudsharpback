using cloudsharpback.Controllers.Base;
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

        [HttpGet("dlToken")]
        public IActionResult GetDownloadToken(string path)
        {
            var err = _memberFileService.GetDownloadTicket(Member, path, Request.HttpContext.Connection.RemoteIpAddress?.ToString(), out var ticket);
            if (err is not null
                || ticket is null)
            {
                return StatusCode(err!.ErrorCode, err.Message);
            }
            _ticketStore.Add(ticket);
            return Ok(ticket.Token.ToString());
        }

        [HttpGet("viewToken")]
        public IActionResult GetViewToken(string path)
        {
            var err = _memberFileService.GetViewTicket(Member, path, Request.HttpContext.Connection.RemoteIpAddress?.ToString(), out var ticket);
            if (err is not null 
                || ticket is null)
            {
                return StatusCode(err!.ErrorCode, err.Message);
            }
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
