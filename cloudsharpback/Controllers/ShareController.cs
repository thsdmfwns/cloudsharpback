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
        private readonly IShareService _shareService;
        private readonly IJWTService _jwtService;
        private readonly ITicketStore _ticketStore;

        public ShareController(IShareService shareService, IJWTService jwtService, ITicketStore ticketStore)
        {
            this._shareService = shareService;
            _jwtService = jwtService;
            _ticketStore = ticketStore;
        }

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

        [HttpGet("ls")]
        public async Task<IActionResult> GetShares()
        {
            var res = await _shareService.GetSharesAsync(Member);
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
            var res = await _shareService.GetShareAsync(token);
            if (res.err is not null || res.result is null)
            {
                return StatusCode(res.err!.HttpCode, res.err.Message);
            }
            return Ok(res.result);
        }

        [AllowAnonymous]
        [HttpPost("dlTicket")]
        public async Task<IActionResult> GetDownloadTicket(ShareDowonloadRequestDto requestDto, [FromHeader] string? auth)
        {
            var result = await _shareService.GetDownloadDtoAsync(requestDto);
            if (result.err is not null || result.dto is null)
            {
                return StatusCode(result.err!.HttpCode, result);
            }
            MemberDto? member = null;
            if (auth is not null)
            {
                _jwtService.TryValidateAcessToken(auth, out member);
            }
            var dl = new DownloadToken(result.dto.Directory, result.dto.Target, DownloadType.Download);
            var ticket = new Ticket(IpAdressUtil.Get(HttpContext), member, TicketType.Download, dl);
            _ticketStore.Add(ticket);
            return Ok(ticket.Token.ToString());
        }

        [HttpPost("close")]
        public async Task<IActionResult> CloseShare(string token)
        {
            if (!Guid.TryParse(token, out _))
            {
                return BadRequest();
            }
            var err = await _shareService.CloseShareAsync(Member, token);
            return err is null ? Ok() : StatusCode(err.HttpCode, err.Message);
        }
        [HttpPost("update")]
        public async Task<IActionResult> UpdateShare(string token, [FromBody] ShareUpdateDto dto)
        {
            if (!Guid.TryParse(token, out _))
            {
                return BadRequest();
            }
            var err = await _shareService.UpdateShareAsync(dto, token, Member);
            return err is null ? Ok() : StatusCode(err.HttpCode, err.Message);
        }

        [AllowAnonymous]
        [HttpPost("validatePw")]
        public async Task<IActionResult> ValidatePassword(ShareRequestValidatePasswordDto dto)
        {
            if (!Guid.TryParse(dto.token, out _))
            {
                return BadRequest();
            }
            var result = await _shareService.ValidatePassword(dto.password, dto.token);
            if (result.err is not null || result.result is null)
            {
                return StatusCode(result.err!.HttpCode, result.err.Message);
            }
            return Ok(result.result);
        }

        [AllowAnonymous]
        [HttpGet("checkPw")]
        public async Task<IActionResult> CheckPassword(string token)
        {
            if (!Guid.TryParse(token, out _))
            {
                return BadRequest();
            }
            var result = await _shareService.CheckPassword(token);
            return Ok(result);
        }
    }
}
