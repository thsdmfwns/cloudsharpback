using cloudsharpback.Controllers.Base;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : AuthControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IShareService _shareService;
        private readonly ITusService _tusService;

        public FileController(IFileService fileService, IShareService shareService, ITusService tusService)
        {
            this._fileService = fileService;
            this._shareService = shareService;
            this._tusService = tusService;
        }

        [HttpGet("files")]
        public IActionResult GetFiles(string? path)
        {
            return Ok(_fileService.GetFiles(Member.Directory, path));
        }

        [ProducesResponseType(404)]
        [HttpGet("file")]
        public IActionResult GetFile(string path)
        {
            if (!_fileService.GetFile(Member, path, out var fileDto))
            {
                return NotFound();
            }

            return Ok(fileDto);
        }

        [HttpGet("dlToken")]
        public IActionResult GetDownloadToken(string path)
        {
            var err = _fileService.GetDownloadToken(Member, path, out var token);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok(token.ToString());
        }

        [HttpGet("viewToken")]
        public IActionResult GetViewToken(string path)
        {
            var err = _fileService.GetViewToken(Member, path, out var token);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok(token.ToString());
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

        [AllowAnonymous]
        [HttpGet("dl/{token}")]
        public IActionResult DownLoad(string token)
        {
            if (!Guid.TryParse(token, out var ticketToken))
            {
                return StatusCode(400, "bad token");
            };
            var err = _fileService.GetFileStream(ticketToken, out var fileStream);
            if (err is not null || fileStream is null)
            {
                return StatusCode(err!.ErrorCode, err.Message);
            }
            return new FileStreamResult(fileStream, MimeTypeUtil.GetMimeType(fileStream.Name) ?? "application/octet-stream")
            {
                FileDownloadName = Path.GetFileName(fileStream.Name),
                EnableRangeProcessing = true
            };
        }

        [AllowAnonymous]
        [HttpGet("view/{token}")]
        public IActionResult View(string token)
        {
            if (!Guid.TryParse(token, out var ticketToken))
            {
                return StatusCode(403, "bad token");
            };
            var err = _fileService.GetFileStream(ticketToken, out var fileStream);
            if (err is not null || fileStream is null)
            {
                return StatusCode(err!.ErrorCode, err.Message);
            }
            return new FileStreamResult(fileStream, MimeTypeUtil.GetMimeType(fileStream.Name) ?? "application/octet-stream")
            {
                EnableRangeProcessing = true
            };
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(string path)
        {
            if (!_fileService.DeleteFile(Member, path, out var fileDto))
            {
                return StatusCode(404);
            }
            await _shareService.DeleteShareAsync(path, Member);
            return Ok(fileDto);
        }
    }
}
