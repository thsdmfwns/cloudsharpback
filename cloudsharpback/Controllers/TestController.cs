using cloudsharpback.Models;
using cloudsharpback.Services;
using cloudsharpback.Utills;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IJWTService jwtService;

        public TestController(IJWTService jwtService)
        {
            this.jwtService = jwtService;
        }

        [HttpPost("encrypt")]
        public string Encrypt(string password)
        {
            return PasswordEncrypt.EncryptPassword(password);
        }

        [HttpPost("tokenCreate")]
        public string TokenCreate(MemberDto member)
        {
            return jwtService.TokenCreate(member);
        }

        [HttpPost("tokenVal")]
        public bool TokenVal(string token)
        {
            return jwtService.TryTokenValidation(token, out var jwt);
        }
    }
}
