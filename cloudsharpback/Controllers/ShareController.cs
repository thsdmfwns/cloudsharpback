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
            
            MemberDto? member = null;
            if (auth is not null)
            {
                _jwtService.TryValidateAccessToken(auth, out member);
            }
            
            var result = await _shareService.GetDownloadTicketValue(requestDto);
            if (result.err is not null || result.ticketValue is null)
            {
                return StatusCode(result.err!.HttpCode, result);
            }
            var ticket = new Ticket(IpAdressUtil.Get(HttpContext), member, TicketType.Download, result.ticketValue);
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
            if (!Guid.TryParse(dto.Token, out _))
            {
                return BadRequest();
            }
            var result = await _shareService.ValidatePassword(dto.Password, dto.Token);
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

        [HttpGet("find")]
        public async Task<IActionResult> FindSharesInDirectory(string path)
        {
            var res = await _shareService.FindSharesInDirectory(Member, path);
            return res.err is null ? Ok(res.shares) : StatusCode(res.err.HttpCode, res.err.Message);
        }
    }
}
