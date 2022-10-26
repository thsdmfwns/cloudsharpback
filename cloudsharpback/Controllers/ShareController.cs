using cloudsharpback.Models;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShareController : ControllerBase
    {
        private readonly IJWTService jwtService;
        private readonly IShareService shareService;

        public ShareController(IJWTService jwtService, IShareService shareService)
        {
            this.jwtService = jwtService;
            this.shareService = shareService;
        }

        [HttpPost("share")]
        public async Task<IActionResult> Share(ShareRequestDto req, [FromHeader] string auth)
        {
            if (!jwtService.TryTokenValidation(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            var token = await shareService.Share(memberDto, req);
            return Ok(token);
        }

        [HttpGet("getList")]
        public async Task<IActionResult> GetShares([FromHeader] string auth)
        {
            if (!jwtService.TryTokenValidation(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            var res = await shareService.GetSharesAsync(memberDto);
            return Ok(res);
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetShare(string token)
        {
            if (!Guid.TryParse(token, out _))
            {
                return BadRequest();
            }
            var res = await shareService.GetShareAsync(token);
            return Ok(res);
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownLoad(string token, string? password)
        {
            if (!Guid.TryParse(token, out _))
            {
                return BadRequest();
            }
            var fs = await shareService.DownloadShareAsync(token, password);
            return new FileStreamResult(fs, "application/octet-stream")
            {
                FileDownloadName = Path.GetFileName(fs.Name),
                EnableRangeProcessing = true
            };
        }

        [HttpPost("close")]
        public async Task<IActionResult> CloseShare(string token, [FromHeader] string auth)
        {
            if (!jwtService.TryTokenValidation(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            var result = await shareService.CloseShareAsync(memberDto, token);
            return result ? Ok() : BadRequest();
        }
    }
}
