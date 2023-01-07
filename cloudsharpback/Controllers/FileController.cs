using cloudsharpback.Attribute;
using cloudsharpback.Controllers.Base;
using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : AuthControllerBase
    {
        private readonly IFileService fileService;
        private readonly IShareService shareService;
        private readonly ITusService tusService;

        public FileController(IFileService fileService, IShareService shareService, ITusService tusService)
        {
            this.fileService = fileService;
            this.shareService = shareService;
            this.tusService = tusService;
        }

        [HttpGet("files")]
        public IActionResult GetFiles(string? path)
        {
            return Ok(fileService.GetFiles(Member!.Directory, path));
        }

        [ProducesResponseType(404)]
        [HttpGet("file")]
        public IActionResult GetFile(string path)
        {
            if (!fileService.GetFile(Member!, path, out var fileDto))
            {
                return NotFound();
            }

            return Ok(fileDto);
        }

        [HttpGet("dlToken")]
        public IActionResult GetDownloadToken(string path)
        {
            var err = fileService.GetDownloadToken(Member!, path, out var token);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok(token.ToString());
        }

        [HttpGet("viewToken")]
        public IActionResult GetViewToken(string path)
        {
            var err = fileService.GetViewToken(Member!, path, out var token);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok(token.ToString());
        }

        [HttpGet("tusToken")]
        public IActionResult GetTusToken()
        {
            var err = tusService.GetTusToken(Member!, out var token);
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
            if (!Guid.TryParse(token, out var id))
            {
                return StatusCode(400, "bad token");
            };
            var err = fileService.DownloadFile(id, out var fileStream);
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
            if (!Guid.TryParse(token, out var id))
            {
                return StatusCode(403, "bad token");
            };
            var err = fileService.ViewFile(id, out var fileStream);
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
            if (!fileService.DeleteFile(Member!, path, out var fileDto))
            {
                return StatusCode(404);
            }
            await shareService.DeleteShareAsync(path, Member!);
            return Ok(fileDto);
        }

        [HttpGet("view/zip")]
        public IActionResult ViewZip(string target)
        {
            var err = fileService.ViewZip(Member!, target, out var entries);
            if (err is not null || entries is null)
            {
                return StatusCode(err!.ErrorCode, err.Message);
            }
            return Ok(entries);
        }
    }
}
