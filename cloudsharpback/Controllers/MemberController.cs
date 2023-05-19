﻿using cloudsharpback.Controllers.Base;
using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class MemberController : AuthControllerBase
    {
        private readonly IJWTService _jwtService;
        private readonly IMemberService _memberService;
        private readonly ITicketStore _ticketStore;

        public MemberController(IJWTService jwtService, IMemberService memberService, ITicketStore ticketStore)
        {
            this._jwtService = jwtService;
            this._memberService = memberService;
            _ticketStore = ticketStore;
        }

        [AllowAnonymous]
        [HttpPost("token")]
        public async Task<IActionResult> GetAccessToken([FromHeader] string rf_token)
        {
            if (!_jwtService.TryValidateRefeshToken(rf_token, out var memberId)
                || memberId is null)
            {
                return StatusCode(403, "bad token");
            }
            var result = await _memberService.GetMemberById(memberId.Value);
            if (result.err is not null || result.result is null)
            {
                return StatusCode(result.err!.ErrorCode, result.err.Message);
            }
            var acToken = _jwtService.WriteAcessToken(result.result);
            return Ok(acToken);
        }

        [HttpGet("member")]
        public IActionResult GetMember()
        {
            return Ok(Member);
        }

        [HttpPost("updateImage")]
        public async Task<IActionResult> UploadProfileImage(IFormFile image)
        {
            var res = await _memberService.UploadProfileImage(image, Member);
            if (res is not null)
            {
                return StatusCode(res.ErrorCode, res.Message);
            }
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("imageDl/{image}")]
        public IActionResult DownloadProfileImage(string image)
        {
            var err = _memberService.DownloadProfileImage(image, out var fileStream, out var contentType);
            if (err is not null || fileStream is null || contentType is null)
            {
                return StatusCode(err!.ErrorCode, err.Message);
            }
            return new FileStreamResult(fileStream, contentType)
            {
                FileDownloadName = Path.GetFileName(fileStream.Name),
                EnableRangeProcessing = true
            };
        }


        [HttpPost("updateNick")]
        public async Task<IActionResult> UpdateNickName(string nickname)
        {
            var err = await _memberService.UpadteNickname(Member, nickname);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }

        [HttpPost("updateEmail")]
        public async Task<IActionResult> UpdateEmail(string email)
        {
            var err = await _memberService.UpadteEmail(Member, email);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }

        [HttpPost("checkPw")]
        public async Task<IActionResult> CheckPassword([FromBody]string password)
        {
            var result = await _memberService.CheckPassword(Member, password);
            if (result.err is not null)
            {
                return StatusCode(result.err.ErrorCode, result.err.Message);
            }
            return Ok(result.result);
        }

        [HttpPost("updatePw")]
        public async Task<IActionResult> UpdatePassword(UpadtePasswordDto dto)
        {
            var err = await _memberService.UpdatePassword(Member, dto);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }
        
        [HttpGet("signalrTicket")]
        public async Task<IActionResult> GetSignalrToken()
        {
            var ip = IpAdressUtil.Get(HttpContext);
            var guid = Guid.NewGuid();
            var ticket = new Ticket(HttpContext, TicketType.SignalrConnect, null);
            _ticketStore.Add(ticket);
            return Ok(guid.ToString());
        }

    }
}
