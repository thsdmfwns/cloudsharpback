using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
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

        public FileController(IFileService fileService, IJWTService jwtService)
        {
            this.fileService = fileService;
            this.jwtService = jwtService;
        }

        [HttpGet("files")]
        public IActionResult GetFiles(string? path, [FromHeader]string auth)
        {
            if (!jwtService.TryTokenValidation(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            return Ok(fileService.GetFiles(memberDto.Directory, path));
        }

        [HttpGet("file")]
        public IActionResult GetFile(string path, [FromHeader] string auth)
        {
            if (!jwtService.TryTokenValidation(auth, out var memberDto)
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

        [HttpGet("download")]
        public IActionResult Download(string path, [FromHeader] string auth)
        {
            if (!jwtService.TryTokenValidation(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }

            if (fileService.DownloadFile(memberDto, path, out var fileStream)
                || fileStream is null)
            {
                return StatusCode(404);
            }

            return new FileStreamResult(fileStream, "application/octet-stream")
            {
                FileDownloadName = Path.GetFileName(fileStream.Name),
                EnableRangeProcessing = true
            };
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file, string? path, [FromHeader] string auth)
        {
            if (!jwtService.TryTokenValidation(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            if (!await fileService.UploadFile(file, memberDto, path))
            {
                return StatusCode(409);
            }
            return Ok();
        }

        [HttpPost("delete")]
        public IActionResult Delete(string path, [FromHeader] string auth)
        {
            if (!jwtService.TryTokenValidation(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            if (!fileService.DeleteFile(memberDto, path, out var fileDto))
            {
                return StatusCode(404);
            }
            return Ok(fileDto);
        }
    }
}
