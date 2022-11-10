using cloudsharpback.Models;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
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

        [ProducesResponseType(200)]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var res = await userService.Login(loginDto);
            if (res.err is not null || res.result is null)
            {
                return StatusCode(res.err!.ErrorCode, res.err.Message);
            }
            return Ok(jwtService.WriteToken(res.result));
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var res = await userService.Register(registerDto, 2);
            if (res.err is not null || res.directoryId is null)
            {
                return StatusCode(res.err!.ErrorCode, res.err.Message);
            }
            fileService.TryMakeTemplateDirectory(res.directoryId);
            return Ok();
        }

        [ProducesResponseType(200)]
        [HttpGet("idcheck")]
        public async Task<IActionResult> IdCkeck(string id)
        {
            return Ok(await userService.IdCheck(id));
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin(RegisterDto registerDto, [FromHeader] string adminToken)
        {
            if (!jwtService.TryValidateToken(adminToken, out var memberDto)
                || memberDto is null
                || memberDto.Role != 999)
            {
                return StatusCode(403);
            }
            var res = await userService.Register(registerDto, 999);
            if (res.err is not null || res.directoryId is null)
            {
                return StatusCode(res.err!.ErrorCode, res.err.Message);
            }
            fileService.TryMakeTemplateDirectory(res.directoryId);
            return Ok();
        }

        [ProducesResponseType(200, Type = typeof(MemberDto))]
        [ProducesResponseType(403, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("member")]
        public IActionResult GetMember([FromHeader] string auth)
        {
            if (!jwtService.TryValidateToken(auth, out var memberDto)
               || memberDto is null)
            {
                return StatusCode(403, "bad auth");
            }
            return Ok(memberDto);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(403, Type = typeof(string))]
        [ProducesResponseType(409, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(415, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpPost("updateImage")]
        public async Task<IActionResult> UploadProfileImage(IFormFile image, [FromHeader] string auth)
        {
            if (!jwtService.TryValidateToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403, "bad auth");
            }
            var res = await userService.UploadProfileImage(image, memberDto);
            if (res is not null)
            {
                return StatusCode(res.ErrorCode, res.Message);
            }
            return Ok();
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
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

        [ProducesResponseType(200)]
        [ProducesResponseType(403, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("updateNick")]
        public async Task<IActionResult> UpdateNickName(string nickname, [FromHeader]string auth)
        {
            if (!jwtService.TryValidateToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403, "bad auth");
            }
            var err = await userService.UpadteNickname(memberDto, nickname);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(403, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("updateEmail")]
        public async Task<IActionResult> UpdateEmail(string email, [FromHeader] string auth)
        {
            if (!jwtService.TryValidateToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403, "bad auth");
            }
            var err = await userService.UpadteEmail(memberDto, email);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }

        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(403, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpPost("checkPw")]
        public async Task<IActionResult> CheckPassword([FromBody]string password, [FromHeader] string auth)
        {
            if (!jwtService.TryValidateToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403, "bad auth");
            }
            var result = await userService.CheckPassword(memberDto, password);
            if (result.err is not null)
            {
                return StatusCode(result.err.ErrorCode, result.err.Message);
            }
            return Ok(result.result);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(403, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpPost("updatePw")]
        public async Task<IActionResult> UpadtePassword([FromBody] UpadtePasswordDto requset, [FromHeader] string auth)
        {
            if (!jwtService.TryValidateToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403, "bad auth");
            }
            var err = await userService.UpdatePassword(memberDto, requset);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }
    }
}
