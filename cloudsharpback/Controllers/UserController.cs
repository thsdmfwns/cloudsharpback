using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserService userService;
        private readonly IJWTService jwtService;
        private readonly IFileService fileService;


        public UserController(IUserService userService, IJWTService jwtService, IFileService fileService)
        {
            this.userService = userService;
            this.jwtService = jwtService;
            this.fileService = fileService;
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
    }
}
