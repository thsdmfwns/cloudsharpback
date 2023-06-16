using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;
        private readonly IJWTService _jwtService;


        public UserController(IUserService userService, IJWTService jwtService)
        {
            this._userService = userService;
            this._jwtService = jwtService;
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _userService.Login(loginDto);
            if (result.err is not null || result.result is null)
            {
                return StatusCode(result.err!.HttpCode, result.err.Message);
            }
            var res = new TokenDto(_jwtService.WriteAccessToken(result.result), _jwtService.WriteRefreshToken(result.result));
            return Ok(res);
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var err = await _userService.Register(registerDto, 2);
            return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok();
        }
        
        [HttpGet("idCheck")]
        public async Task<IActionResult> IdCheck(string id)
        {
            return Ok(await _userService.IdCheck(id));
        }
    }
}
