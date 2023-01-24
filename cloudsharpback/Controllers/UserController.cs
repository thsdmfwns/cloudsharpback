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

        private readonly IUserService _userService;
        private readonly IJWTService _jwtService;
        private readonly IFileService _fileService;


        public UserController(IUserService userService, IJWTService jwtService, IFileService fileService)
        {
            this._userService = userService;
            this._jwtService = jwtService;
            this._fileService = fileService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _userService.Login(loginDto);
            if (result.err is not null || result.result is null)
            {
                return StatusCode(result.err!.ErrorCode, result.err.Message);
            }
            var res = new TokenDto(_jwtService.WriteAcessToken(result.result), _jwtService.WriteRefeshToken(result.result));
            return Ok(res);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var res = await _userService.Register(registerDto, 2);
            if (res.err is not null || res.directoryId is null)
            {
                return StatusCode(res.err!.ErrorCode, res.err.Message);
            }
            _fileService.MakeTemplateDirectory(res.directoryId);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("id_check")]
        public async Task<IActionResult> IdCheck(string id)
        {
            return Ok(await _userService.IdCheck(id));
        }
    }
}
