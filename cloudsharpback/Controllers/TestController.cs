using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
using Microsoft.AspNetCore.Mvc;
using System.IO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IJWTService jwtService;
        private readonly IUserService userService;
        private readonly IFileService fileService;
        private readonly IShareService shareService;

        public TestController(IJWTService jwtService, IUserService userService, ILogger<TestController> logger, IFileService fileService, IShareService shareService)
        {
            this.jwtService = jwtService;
            this.userService = userService;
            this.fileService = fileService;
            this.shareService = shareService;
        }
    }

}
