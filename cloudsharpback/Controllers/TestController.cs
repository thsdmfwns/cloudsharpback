﻿using cloudsharpback.Models;
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
        private readonly IUserService userService;

        public TestController(IJWTService jwtService, IUserService userService)
        {
            this.jwtService = jwtService;
            this.userService = userService;
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

        [HttpPost("Login")]
        public bool Login(LoginDto loginDto)
        {
            return userService.TryLogin(loginDto, out var jwt);
        }

        [HttpPost("Register")]
        public bool Register(RegisterDto dto)
        {
            return userService.TryRegister(dto);
        }

        [HttpPost("Idcheck1")]
        public bool Idcheck1(string id)
        {
            return userService.IdCheck(id, out var temp);
        }

        [HttpPost("Idcheck2")]
        public bool Idcheck2(string id)
        {
            return userService.IdCheck(id);
        }
    }
}
