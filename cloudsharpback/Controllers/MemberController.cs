using cloudsharpback.Controllers.Base;
using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.FIle;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Models.Ticket;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace cloudsharpback.Controllers
{
    [Route("api/Member")]
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

        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(string))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "fail to login")]
        [AllowAnonymous]
        [HttpPost("token")]
        public async Task<IActionResult> GetAccessToken([FromHeader] string auth)
        {
            if (!_jwtService.TryValidateRefreshToken(auth, out var memberId)
                || memberId is null)
            {
                return StatusCode(403, "bad token");
            }
            var result = await _memberService.GetMemberById(memberId.Value);
            if (result.err is not null || result.result is null)
            {
                return StatusCode(result.err!.HttpCode, result.err.Message);
            }
            var acToken = _jwtService.WriteAccessToken(result.result);
            return Ok(acToken);
        }

        [HttpGet("get")]
        public IActionResult GetMember()
        {
            return Ok(Member);
        }

        
        [SwaggerResponse(StatusCodes.Status200OK, "success")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "member not found")]
        [SwaggerResponse(StatusCodes.Status409Conflict, "try again")]
        [SwaggerResponse(StatusCodes.Status415UnsupportedMediaType, "bad type")]
        [HttpPost("updateProfile")]
        public async Task<IActionResult> UploadProfileImage(IFormFile image)
        {
            var res = await _memberService.UploadProfileImage(image, Member);
            if (res is not null)
            {
                return StatusCode(res.HttpCode, res.Message);
            }
            return Ok();
        }

        [AllowAnonymous]
        
        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(FileStreamResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "file not found")]
        [HttpGet("profile/{image}")]
        public IActionResult DownloadProfileImage(string image)
        {
            var err = _memberService.DownloadProfileImage(image, out var fileStream, out var contentType);
            if (err is not null || fileStream is null || contentType is null)
            {
                return StatusCode(err!.HttpCode, err.Message);
            }
            return new FileStreamResult(fileStream, contentType)
            {
                FileDownloadName = Path.GetFileName(fileStream.Name),
                EnableRangeProcessing = true
            };
        }


        [SwaggerResponse(StatusCodes.Status200OK, "success")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "member not found")]
        [HttpPost("updateNick")]
        public async Task<IActionResult> UpdateNickName(string nickname)
        {
            var err = await _memberService.UpdateNickname(Member, nickname);
            if (err is not null)
            {
                return StatusCode(err.HttpCode, err.Message);
            }
            return Ok();
        }

        [SwaggerResponse(StatusCodes.Status200OK, "success")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "member not found")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "bad email")]
        [HttpPost("updateEmail")]
        public async Task<IActionResult> UpdateEmail(string email)
        {
            var err = await _memberService.UpdateEmail(Member, email);
            if (err is not null)
            {
                return StatusCode(err.HttpCode, err.Message);
            }
            return Ok();
        }

        
        [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(bool))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "member not found")]
        [HttpPost("checkPw")]
        public async Task<IActionResult> CheckPassword([FromBody]string password)
        {
            var result = await _memberService.CheckPassword(Member, password);
            if (result.err is not null)
            {
                return StatusCode(result.err.HttpCode, result.err.Message);
            }
            return Ok(result.result);
        }

        [SwaggerResponse(StatusCodes.Status200OK, "success")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "member not found")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "bad password")]
        [HttpPost("updatePw")]
        public async Task<IActionResult> UpdatePassword(UpadtePasswordDto dto)
        {
            var err = await _memberService.UpdatePassword(Member, dto);
            if (err is not null)
            {
                return StatusCode(err.HttpCode, err.Message);
            }
            return Ok();
        }
        
        [HttpGet("signalrTicket")]
        public async Task<IActionResult> GetSignalrToken()
        {
            var ticket = new SignalrTicket()
            {
                Owner = Member
            };
            await _ticketStore.AddTicket(ticket);
            return Ok(ticket.Token.ToString());
        }
        
    }
}
