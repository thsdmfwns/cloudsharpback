using cloudsharpback.Controllers.Base;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.FIle;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : AuthControllerBase
    {
        private readonly IMemberFileService _memberFileService;
        private readonly ITicketStore _ticketStore;
        private readonly IShareService _shareService;

        public FileController(IMemberFileService memberFileService, IShareService shareService,
            ITicketStore ticketStore)
        {
            this._memberFileService = memberFileService;
            this._shareService = shareService;
            _ticketStore = ticketStore;
        }
        
        /// <response code="404">directory not found</response>
        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(List<FileInfoDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "directory not found")]
        [HttpGet("ls")]
        public IActionResult GetFileDtoList(string? path, bool? onlyDir)
        {
            var err = _memberFileService.GetFiles(Member, path, out var files, onlyDir ?? false);
            return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok(files);
        }

        /// <response code="404">file not found</response>
        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(FileInfoDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "file not found")]
        [HttpGet("get")]
        public IActionResult GetFileDto(string path)
        {
            var err = _memberFileService.GetFile(Member, path, out var fileDto);
            return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok(fileDto);
        }

        /// <response code="404">directory not found</response>
        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "directory not found")]
        [HttpGet("dlTicket")]
        public async Task<IActionResult> GetDownloadTicket(string path)
        {
            var err = _memberFileService.GetDownloadTicket(Member, path, out var ticket);
            if (err is not null)
            {
                return StatusCode(err.HttpCode, err.Message);
            }
            
            await _ticketStore.AddTicket(ticket!);
            return Ok(ticket!.Token.ToString());
        }
        
        
        /// <response code="404">directory not found</response>
        /// <response code="415">file can't view</response>
        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "directory not found")]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "file can't view")]
        [HttpGet("viTicket")]
        public async Task<IActionResult> GetViewTicket(string path)
        {
            var err = _memberFileService.GetDownloadTicket(Member, path, out var ticket, true);
            if (err is not null)
            {
                return StatusCode(err.HttpCode, err.Message);
            }
            await _ticketStore.AddTicket(ticket!);
            return Ok(ticket!.Token.ToString());
        }
        
        /// <response code="404">directory not found</response>
        /// <response code="409">same filename exist</response>
        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "same filename exist")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "directory not found")]
        [HttpPost("ulTicket")]
        public async Task<IActionResult> GetUploadToken(FileUploadRequestDto requestDto)
        {
            var err = _memberFileService.GetUploadTicket(Member, requestDto, out var ticket);
            if (err is not null)
            {
                return StatusCode(err.HttpCode, err.Message);
            }
            await _ticketStore.AddTicket(ticket!);
            return Ok(ticket!.Token.ToString());
        }
        
        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(List<FileInfoDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "share not found")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "directory or file not found")]
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
        
        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(List<FileInfoDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "directory not found")]
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
        
        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(List<FileInfoDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "root directory not found")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "same directory name exist")]
        [HttpPost("mkdir")]
        public IActionResult MakeDirectory(string? rootDir, string dirName)
        {
            var err = _memberFileService.MakeDirectory(Member, rootDir, dirName, out var fileDtos);
            return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok(fileDtos);
        }
    }
}
