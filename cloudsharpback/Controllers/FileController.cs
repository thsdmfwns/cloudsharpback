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

        public FileController(IMemberFileService memberFileService, IShareService shareService, ITicketStore ticketStore)
        {
            this._memberFileService = memberFileService;
            this._shareService = shareService;
            _ticketStore = ticketStore;
        }

        [HttpGet("ls")]
        public IActionResult GetFileDtoList(string? path)
        {
            var err = _memberFileService.GetFiles(Member, path, out var files);
            return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok(files);
        }

        [ProducesResponseType(404)]
        [HttpGet("get")]
        public IActionResult GetFileDto(string path)
        {
            var err = _memberFileService.GetFile(Member, path, out var fileDto);
            return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok(fileDto);
        }

        [HttpGet("dlTicket")]
        public IActionResult GetDownloadTicket(string path)
        {
            var err = _memberFileService.CheckBeforeDownloadTicketAdd(Member, path);
            if (err is not null)
            {
                return StatusCode(err.HttpCode, err.Message);
            }
            var dl = new DownloadToken(Member.Directory, path, DownloadType.Download);
            var ticket = new Ticket(HttpContext, TicketType.Download, dl);
            _ticketStore.Add(ticket);
            return Ok(ticket.Token.ToString());
        }

        [HttpGet("viTicket")]
        public IActionResult GetViewTicket(string path)
        {
            var err = _memberFileService.CheckBeforeDownloadTicketAdd(Member, path, true);
            if (err is not null)
            {
                return StatusCode(err.HttpCode, err.Message);
            }
            var dl = new DownloadToken(Member.Directory, path, DownloadType.View);
            var ticket = new Ticket(HttpContext, TicketType.ViewFile, dl);
            _ticketStore.Add(ticket);
            return Ok(ticket.Token.ToString());
        }

        [HttpPost("ulTicket")]
        public IActionResult GetUploadToken(FileUploadDto dto)
        {
            var err = _memberFileService.CheckBeforeUploadTicketAdd(Member, dto);
            if (err is not null)
            {
                return StatusCode(err.HttpCode, err.Message);
            }
            var token = new TusUploadToken()
            {
                FileName = dto.FileName,
                FileDirectory = Member.Directory,
                FilePath = dto.UploadDirectory ?? string.Empty
            };
            var ticket = new Ticket(HttpContext, DateTime.Now.AddDays(3), TicketType.TusUpload, token);
            _ticketStore.Add(ticket);
            return Ok(ticket.Token.ToString());
        }

        [HttpPost("rm")]
        public async Task<IActionResult> DeleteFile(string path)
        {
            HttpResponseDto? err;
            if (await _shareService.CheckExistShareByTargetPath(path, Member))
            {
                err =await _shareService.DeleteShareAsync(path, Member);
                if (err is not null)
                {
                    return StatusCode(err.HttpCode, err.Message);
                }
            }
            err = _memberFileService.DeleteFile(Member, path, out var fileDto);
            return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok(fileDto);
        }
        
        [HttpPost("rmdir")]
        public async Task<IActionResult> DeleteDirectory(string path)
        {
            var err =await _shareService.DeleteSharesInDirectory(Member, path);
            if (err is not null)
            {
                return StatusCode(err.HttpCode, err.Message);
            }
            err = _memberFileService.RemoveDirectory(Member, path, out var fileDto);
            return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok(fileDto);
        }
        

        [HttpPost("mkdir")]
        public IActionResult MakeDirectory(string? rootDir, string dirName)
        {
            var err = _memberFileService.MakeDirectory(Member, rootDir, dirName, out var fileDtos);
            return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok(fileDtos);
        }
    }
}
