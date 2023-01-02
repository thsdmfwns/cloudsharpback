﻿using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
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

        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpPost("share")]
        public async Task<IActionResult> Share(ShareRequestDto req, [FromHeader] string auth)
        {
            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            var result = await shareService.Share(memberDto, req);
            if (result is not null)
            {
                return StatusCode(result.ErrorCode, result.Message);
            }
            return Ok();
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("getList")]
        public async Task<IActionResult> GetShares([FromHeader] string auth)
        {
            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            var res = await shareService.GetSharesAsync(memberDto);
            return Ok(res);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(410, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
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

        [ProducesResponseType(200)]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(403, Type = typeof(string))]
        [ProducesResponseType(410, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
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

        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
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

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpPost("close")]
        public async Task<IActionResult> CloseShare(string token, [FromHeader] string auth)
        {
            if (!Guid.TryParse(token, out _))
            {
                return BadRequest();
            }
            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            var result = await shareService.CloseShareAsync(memberDto, token);
            return result ? Ok() : NotFound();
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpPost("update")]
        public async Task<IActionResult> UpadteShare(string token, [FromBody] ShareUpdateDto dto, [FromHeader] string auth)
        {
            if (!Guid.TryParse(token, out _))
            {
                return BadRequest();
            }
            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            var result = await shareService.UpdateShareAsync(dto, token, memberDto);
            return result ? Ok() : NotFound();
        }


        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
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

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500, Type = typeof(string))]
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
