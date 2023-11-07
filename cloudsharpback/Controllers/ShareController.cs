using cloudsharpback.Controllers.Base;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Models.DTO.Share;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShareController : AuthControllerBase
    {
        private readonly IShareService _shareService;
        private readonly IJWTService _jwtService;
        private readonly ITicketStore _ticketStore;

        public ShareController(IShareService shareService, IJWTService jwtService, ITicketStore ticketStore)
        {
            this._shareService = shareService;
            _jwtService = jwtService;
            _ticketStore = ticketStore;
        }

        [SwaggerResponse(StatusCodes.Status400BadRequest, "file not found")]
        [SwaggerResponse(StatusCodes.Status200OK, "success")]
        [HttpPost("share")]
        public async Task<IActionResult> Share(ShareRequestDto req)
        {
            var result = await _shareService.Share(Member, req);
            if (result is not null)
            {
                return StatusCode(result.HttpCode, result.Message);
            }
            return Ok();
        }

        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(List<ShareResponseDto>))]
        [HttpGet("ls")]
        public async Task<IActionResult> GetShares()
        {
            var res = await _shareService.GetSharesAsync(Member);
            return Ok(res);
        }

        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(ShareResponseDto))]
        [SwaggerResponse(StatusCodes.Status410Gone, "expired")]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status404NotFound, "share not found")]
        [AllowAnonymous]
        [HttpGet("get")]
        public async Task<IActionResult> GetShare(string token)
        {
            if (!Guid.TryParse(token, out var guidToken))
            {
                return BadRequest();
            }
            var res = await _shareService.GetShareAsync(guidToken);
            if (res.err is not null || res.result is null)
            {
                return StatusCode(res.err!.HttpCode, res.err.Message);
            }
            return Ok(res.result);
        }

        
        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status410Gone, "expired")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "bad password")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "share not found")]
        [AllowAnonymous]
        [HttpPost("dlTicket")]
        public async Task<IActionResult> GetDownloadTicket(ShareDowonloadRequestDto requestDto, [FromHeader] string? auth)
        {
            
            MemberDto? member = null;
            if (auth is not null)
            {
                _jwtService.TryValidateAccessToken(auth, out member);
            }
            
            var result = await _shareService.GetDownloadTicketValue(requestDto, member);
            if (result.err is not null || result.ticket is null)
            {
                return StatusCode(result.err!.HttpCode, result);
            }
            await _ticketStore.AddTicket(result.ticket);
            return Ok(result.ticket.Token.ToString());
        }
        
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound, "share not found")]
        [HttpPost("close")]
        public async Task<IActionResult> CloseShare(string token)
        {
            if (!Guid.TryParse(token, out var guidToken))
            {
                return BadRequest();
            }
            var err = await _shareService.CloseShareAsync(Member, guidToken);
            return err is null ? Ok() : StatusCode(err.HttpCode, err.Message);
        }
        
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound, "share not found")]
        [HttpPost("update")]
        public async Task<IActionResult> UpdateShare(string token, [FromBody] ShareUpdateDto dto)
        {
            if (!Guid.TryParse(token, out var guidToken))
            {
                return BadRequest();
            }
            var err = await _shareService.UpdateShareAsync(dto, guidToken, Member);
            return err is null ? Ok() : StatusCode(err.HttpCode, err.Message);
        }

        
        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(bool))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "share not found")]
        [AllowAnonymous]
        [HttpPost("validatePw")]
        public async Task<IActionResult> ValidatePassword(ShareRequestValidatePasswordDto dto)
        {
            if (!Guid.TryParse(dto.Token, out var guidToken))
            {
                return BadRequest();
            }
            var result = await _shareService.ValidatePassword(dto.Password, guidToken);
            if (result.err is not null || result.result is null)
            {
                return StatusCode(result.err!.HttpCode, result.err.Message);
            }
            return Ok(result.result);
        }

        
        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(bool))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "share not found")]
        [AllowAnonymous]
        [HttpGet("checkPw")]
        public async Task<IActionResult> CheckPassword(string token)
        {
            if (!Guid.TryParse(token, out var guidToken))
            {
                return BadRequest();
            }
            var result = await _shareService.CheckPassword(guidToken);
            return Ok(result);
        }

        [HttpGet("find")]
        public async Task<IActionResult> FindSharesInDirectory(string path)
        {
            var res = await _shareService.FindSharesInDirectory(Member, path);
            return res.err is null ? Ok(res.shares) : StatusCode(res.err.HttpCode, res.err.Message);
        }
    }
}
