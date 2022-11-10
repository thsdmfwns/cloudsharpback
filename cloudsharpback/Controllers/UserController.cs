using cloudsharpback.Models;
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
        [HttpPost("profileImage")]
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
    }
}
