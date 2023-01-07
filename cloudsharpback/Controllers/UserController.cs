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
    public class UserController : AuthControllerBase
    {
        private readonly IJWTService jwtService;
        private readonly IUserService userService;
        private readonly IFileService fileService;

        public UserController(IJWTService jwtService, IUserService userService, IFileService fileService)
        {
            this.jwtService = jwtService;
            this.userService = userService;
            this.fileService = fileService;
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
            var result = await userService.GetMemberById(memberId.Value);
            if (result.err is not null || result.result is null)
            {
                return StatusCode(result.err!.ErrorCode, result.err.Message);
            }
            var acToken = jwtService.WriteAcessToken(result.result);
            return Ok(acToken);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await userService.Login(loginDto);
            if (result.err is not null || result.result is null)
            {
                return StatusCode(result.err!.ErrorCode, result.err.Message);
            }
            var res = new TokenDto(jwtService.WriteAcessToken(result.result), jwtService.WriteRefeshToken(result.result));
            return Ok(res);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var res = await userService.Register(registerDto, 2);
            if (res.err is not null || res.directoryId is null)
            {
                return StatusCode(res.err!.ErrorCode, res.err.Message);
            }
            fileService.MakeTemplateDirectory(res.directoryId);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("idcheck")]
        public async Task<IActionResult> IdCkeck(string id)
        {
            return Ok(await userService.IdCheck(id));
        }

        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin(RegisterDto registerDto)
        {
            if (Member.Role != 999)
            {
                return StatusCode(403);
            }
            var res = await userService.Register(registerDto, 999);
            if (res.err is not null || res.directoryId is null)
            {
                return StatusCode(res.err!.ErrorCode, res.err.Message);
            }
            fileService.MakeTemplateDirectory(res.directoryId);
            return Ok();
        }

        [HttpGet("member")]
        public IActionResult GetMember()
        {
            return Ok(Member);
        }

        [HttpPost("updateImage")]
        public async Task<IActionResult> UploadProfileImage(IFormFile image)
        {
            var res = await userService.UploadProfileImage(image, Member);
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
            var err = userService.DownloadProfileImage(image, out var fileStream, out var contentType);
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
            var err = await userService.UpadteNickname(Member, nickname);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }

        [HttpPost("updateEmail")]
        public async Task<IActionResult> UpdateEmail(string email)
        {
            var err = await userService.UpadteEmail(Member, email);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }

        [HttpPost("checkPw")]
        public async Task<IActionResult> CheckPassword([FromBody]string password)
        {
            var result = await userService.CheckPassword(Member, password);
            if (result.err is not null)
            {
                return StatusCode(result.err.ErrorCode, result.err.Message);
            }
            return Ok(result.result);
        }

        [HttpPost("updatePw")]
        public async Task<IActionResult> UpadtePassword(UpadtePasswordDto requset)
        {
            var err = await userService.UpdatePassword(Member, requset);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }
    }
}
