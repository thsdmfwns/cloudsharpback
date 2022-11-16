using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileService fileService;
        private readonly IJWTService jwtService;
        private readonly IShareService shareService;
        private readonly ITusService tusService;

        public FileController(IFileService fileService, IJWTService jwtService, IShareService shareService, ITusService tusService)
        {
            this.fileService = fileService;
            this.jwtService = jwtService;
            this.shareService = shareService;
            this.tusService = tusService;
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(500, Type = typeof(string))]
        [ProducesResponseType(403)]
        [HttpGet("files")]
        public IActionResult GetFiles(string? path, [FromHeader]string auth)
        {
            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            return Ok(fileService.GetFiles(memberDto.Directory, path));
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500, Type = typeof(string))]
        [ProducesResponseType(404)]
        [HttpGet("file")]
        public IActionResult GetFile(string path, [FromHeader] string auth)
        {
            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            if (!fileService.GetFile(memberDto, path, out var fileDto))
            {
                return NotFound();
            }

            return Ok(fileDto);
        }

        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(403, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(409, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("dlToken")]
        public IActionResult GetDownloadToken(string path, [FromHeader] string auth)
        {
            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403, "bad auth");
            }
            var err = fileService.GetDownloadToken(memberDto, path, out var token);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok(token.ToString());
        }

        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(403, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(409, Type = typeof(string))]
        [ProducesResponseType(415, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("viewToken")]
        public IActionResult GetViewToken(string path, [FromHeader] string auth)
        {
            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403, "bad auth");
            }
            var err = fileService.GetViewToken(memberDto, path, out var token);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok(token.ToString());
        }

        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(403, Type = typeof(string))]
        [ProducesResponseType(409, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("tusToken")]
        public IActionResult GetTusToken([FromHeader] string auth)
        {
            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403, "bad auth");
            }
            var err = tusService.GetTusToken(memberDto, out var token);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok(token.ToString());
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(403, Type = typeof(string))]
        [ProducesResponseType(410, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("dl/{token}")]
        public IActionResult DownLoad(string token)
        {
            if (!Guid.TryParse(token, out var id))
            {
                return StatusCode(403, "bad token");
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

        [ProducesResponseType(200)]
        [ProducesResponseType(403, Type = typeof(string))]
        [ProducesResponseType(410, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
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
                FileDownloadName = Path.GetFileName(fileStream.Name),
                EnableRangeProcessing = true
            };
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500, Type = typeof(string))]
        [ProducesResponseType(404)]
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(string path, [FromHeader] string auth)
        {
            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            if (!fileService.DeleteFile(memberDto, path, out var fileDto))
            {
                return StatusCode(404);
            }
            await shareService.DeleteShareAsync(path, memberDto);
            return Ok(fileDto);
        }
    }
}
