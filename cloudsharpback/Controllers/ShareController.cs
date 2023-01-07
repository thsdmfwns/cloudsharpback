using cloudsharpback.Controllers.Base;
using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShareController : AuthControllerBase
    {
        private readonly IShareService shareService;

        public ShareController(IShareService shareService)
        {
            this.shareService = shareService;
        }

        [HttpPost("share")]
        public async Task<IActionResult> Share(ShareRequestDto req)
        {
            var result = await shareService.Share(Member, req);
            if (result is not null)
            {
                return StatusCode(result.ErrorCode, result.Message);
            }
            return Ok();
        }

        [HttpGet("getList")]
        public async Task<IActionResult> GetShares()
        {
            var res = await shareService.GetSharesAsync(Member);
            return Ok(res);
        }

        [AllowAnonymous]
        [HttpGet("get")]
        public async Task<IActionResult> GetShare(string token)
        {
            if (!Guid.TryParse(token, out _))
            {
                return BadRequest();
            }
            var res = await shareService.GetShareAsync(token);
            if (res.err is not null || res.result is null)
            {
                return StatusCode(res.err!.ErrorCode, res.err.Message);
            }
            return Ok(res.result);
        }

        [AllowAnonymous]
        [HttpPost("dlToken")]
        public async Task<IActionResult> GetDownloadToken(ShareDowonloadRequestDto requestDto)
        {
            var result = await shareService.GetDownloadTokenAsync(requestDto);
            if (result.err is not null || result.dlToken is null)
            {
                return StatusCode(result.err!.ErrorCode, result);
            }
            return Ok(result.dlToken.ToString());
        }

        [AllowAnonymous]
        [HttpGet("dl/{token}")]
        public IActionResult Donload(string token)
        {
            var err = shareService.DownloadShare(token, out var fileStream);
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

        [HttpPost("close")]
        public async Task<IActionResult> CloseShare(string token)
        {
            if (!Guid.TryParse(token, out _))
            {
                return BadRequest();
            }
            var result = await shareService.CloseShareAsync(Member, token);
            return result ? Ok() : NotFound();
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpadteShare(string token, [FromBody] ShareUpdateDto dto)
        {
            if (!Guid.TryParse(token, out _))
            {
                return BadRequest();
            }
            var result = await shareService.UpdateShareAsync(dto, token, Member);
            return result ? Ok() : NotFound();
        }

        [AllowAnonymous]
        [HttpPost("val")]
        public async Task<IActionResult> ValidatePassword(string token, string password)
        {
            if (!Guid.TryParse(token, out _))
            {
                return BadRequest();
            }
            var result = await shareService.ValidatePassword(password, token);
            if (result.err is not null || result.result is null)
            {
                return StatusCode(result.err!.ErrorCode, result.err.Message);
            }
            return Ok(result.result);
        }

        [AllowAnonymous]
        [HttpGet("check")]
        public async Task<IActionResult> CheckPassword(string token)
        {
            if (!Guid.TryParse(token, out _))
            {
                return BadRequest();
            }
            var result = await shareService.CheckPassword(token);
            return Ok(result);
        }
    }
}
