using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;

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
            if (!jwtService.TryValidateToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            var result = await shareService.Share(memberDto, req);
            return StatusCode(result.ErrorCode, result.Message);
        }

        [HttpGet("getList")]
        public async Task<IActionResult> GetShares([FromHeader] string auth)
        {
            if (!jwtService.TryValidateToken(auth, out var memberDto)
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
            if (!res.response.IsSuccess || res.result is null)
            {
                return StatusCode(res.response.ErrorCode, res);
            }
            return Ok(res.result);
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownLoad(string token, string? password)
        {
            if (!Guid.TryParse(token, out _))
            {
                return BadRequest();
            }
            var result = await shareService.DownloadShareAsync(token, password);
            if (!result.response.IsSuccess || result.result is null)
            {
                return StatusCode(result.response.ErrorCode, result);
            }
            return new FileStreamResult(result.result, "application/octet-stream")
            {
                FileDownloadName = Path.GetFileName(result.result.Name),
                EnableRangeProcessing = true
            };
        }

        [HttpPost("close")]
        public async Task<IActionResult> CloseShare(string token, [FromHeader] string auth)
        {
            if (!Guid.TryParse(token, out _))
            {
                return BadRequest();
            }
            if (!jwtService.TryValidateToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            var result = await shareService.CloseShareAsync(memberDto, token);
            return result ? Ok() : NotFound();
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpadteShare(string token, [FromBody] ShareUpdateDto dto, [FromHeader] string auth)
        {
            if (!Guid.TryParse(token, out _))
            {
                return BadRequest();
            }
            if (!jwtService.TryValidateToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            var result = await shareService.UpdateShareAsync(dto, token, memberDto);
            return result ? Ok() : NotFound();
        }
    }
}
