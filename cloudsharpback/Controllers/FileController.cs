using cloudsharpback.Controllers.Base;
using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.FIle;
using cloudsharpback.Models.Ticket;
using cloudsharpback.Repository.Interface;
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
        public IActionResult GetFileDtoList(string? path, bool? onlyDir)
        {
            var err = _memberFileService.GetFiles(Member, path, out var files, onlyDir ?? false);
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
        public async Task<IActionResult> GetDownloadTicket(string path)
        {
            var err = _memberFileService.GetDownloadTicket(Member, path, out var ticket);
            if (err is not null)
            {
                return StatusCode(err.HttpCode, err.Message);
            }
            
            await _ticketStore.TryAddDownloadTicketAsync(ticket!);
            return Ok(ticket!.Token.ToString());
        }

        [HttpGet("viTicket")]
        public async Task<IActionResult> GetViewTicket(string path)
        {
            var err = _memberFileService.GetDownloadTicket(Member, path, out var ticket, true);
            if (err is not null)
            {
                return StatusCode(err.HttpCode, err.Message);
            }
            await _ticketStore.TryAddDownloadTicketAsync(ticket!);
            return Ok(ticket!.Token.ToString());
        }

        [HttpPost("ulTicket")]
        public async Task<IActionResult> GetUploadToken(FileUploadRequestDto requestDto)
        {
            var err = _memberFileService.GetUploadTicket(Member, requestDto, out var ticket);
            if (err is not null)
            {
                return StatusCode(err.HttpCode, err.Message);
            }
            await _ticketStore.TryAddUpLoadTicketAsync(ticket!);
            return Ok(ticket!.Token.ToString());
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
