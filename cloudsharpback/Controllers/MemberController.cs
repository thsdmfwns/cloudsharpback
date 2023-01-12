using cloudsharpback.Controllers.Base;
using cloudsharpback.Models;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class MemberController : AuthControllerBase
    {
        private readonly IJWTService jwtService;
        private readonly IMemberService memberService;

        public MemberController(IJWTService jwtService, IMemberService memberService)
        {
            this.jwtService = jwtService;
            this.memberService = memberService;
        }

        [AllowAnonymous]
        [HttpPost("token")]
        public async Task<IActionResult> GetAcessToken([FromHeader] string rf_token)
        {
            if (!jwtService.TryValidateRefeshToken(rf_token, out var memberId)
                || memberId is null)
            {
                return StatusCode(403, "bad token");
            }
            var result = await memberService.GetMemberById(memberId.Value);
            if (result.err is not null || result.result is null)
            {
                return StatusCode(result.err!.ErrorCode, result.err.Message);
            }
            var acToken = jwtService.WriteAcessToken(result.result);
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
            var res = await memberService.UploadProfileImage(image, Member);
            if (res is not null)
            {
                return StatusCode(res.ErrorCode, res.Message);
            }
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("imageDL/{image}")]
        public IActionResult DownloadProfileImage(string image)
        {
            var err = memberService.DownloadProfileImage(image, out var fileStream, out var contentType);
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
            var err = await memberService.UpadteNickname(Member, nickname);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }

        [HttpPost("updateEmail")]
        public async Task<IActionResult> UpdateEmail(string email)
        {
            var err = await memberService.UpadteEmail(Member, email);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }

        [HttpPost("checkPw")]
        public async Task<IActionResult> CheckPassword([FromBody]string password)
        {
            var result = await memberService.CheckPassword(Member, password);
            if (result.err is not null)
            {
                return StatusCode(result.err.ErrorCode, result.err.Message);
            }
            return Ok(result.result);
        }

        [HttpPost("updatePw")]
        public async Task<IActionResult> UpadtePassword(UpadtePasswordDto requset)
        {
            var err = await memberService.UpdatePassword(Member, requset);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }
    }
}
