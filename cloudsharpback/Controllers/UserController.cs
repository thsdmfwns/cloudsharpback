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

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var res = await userService.Login(loginDto);
            if (!res.response.IsSuccess || res.result is null)
            {
                return StatusCode(res.response.ErrorCode, res.response.Message);
            }
            return Ok(new { Token = jwtService.WriteToken(res.result) });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var res = await userService.Register(registerDto, 2);
            if (!res.response.IsSuccess || res.directoryId is null)
            {
                return StatusCode(res.response.ErrorCode, res.response.Message);
            }
            fileService.TryMakeTemplateDirectory(res.directoryId);
            return Ok();
        }

        [HttpGet("idcheck")]
        public async Task<IActionResult> IdCkeck(string id)
        {
            return Ok(new
            {
                exist = await userService.IdCheck(id)
            });
        }

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
            if (!res.response.IsSuccess || res.directoryId is null)
            {
                return StatusCode(res.response.ErrorCode, res.response.Message);
            }
            fileService.TryMakeTemplateDirectory(res.directoryId);
            return Ok();
        }

    }
}
