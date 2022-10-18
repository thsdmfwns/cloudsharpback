using cloudsharpback.Models;
using cloudsharpback.Services;
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
        public IActionResult Login(LoginDto loginDto)
        {
            if (!userService.TryLogin(loginDto, out var member)
                || member is null)
            {
                return Unauthorized();
            }
            if (!jwtService.TryTokenCreate(member, out var token)
                || token is null)
            {
                return StatusCode(500, "Fail to Create token");
            }
            return Ok(new { Token = token });
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterDto registerDto)
        {
            if (userService.IdCheck(registerDto.Id))
            {
                return StatusCode(400);
            }
            if (!userService.TryRegister(registerDto, 2, out var dir)
                || dir is null)
            {
                return StatusCode(500, "Fail to Register");
            }
            if (!fileService.TryMakeTemplateDirectory(dir))
            {
                return StatusCode(500, "Fail to Make Directory");
            }
            return Ok();
        }

        [HttpPost("idcheck")]
        public IActionResult IdCkeck(string id)
        {
            return Ok(new
            {
                exist = userService.IdCheck(id)
            });
        }

        [HttpPost("register/admin")]
        public IActionResult RegisterAdmin(RegisterDto registerDto, [FromHeader] string adminToken)
        {
            if (!jwtService.TryTokenValidation(adminToken, out var memberDto)
                || memberDto is null
                || memberDto.Role != 999)
            {
                return StatusCode(403);
            }
            if (userService.IdCheck(registerDto.Id))
            {
                return StatusCode(400);
            }
            if (!userService.TryRegister(registerDto, 999, out var dir)
                || dir is null)
            {
                return StatusCode(500, "Fail to Register");
            }
            if (!fileService.TryMakeTemplateDirectory(dir))
            {
                return StatusCode(500, "Fail to Make Directory");
            }
            return Ok();
        }

    }
}
