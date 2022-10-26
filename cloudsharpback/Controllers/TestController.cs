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

        [HttpPost("encrypt")]
        public string Encrypt(string password)
        {
            return Utills.Encrypt.EncryptByBCrypt(password);
        }

        [HttpPost("tokenCreate")]
        public bool TokenCreate(MemberDto member)
        {
            return jwtService.TryTokenCreate(member, out var token);
        }

        [HttpPost("tokenVal")]
        public IActionResult TokenVal(string token)
        {
            jwtService.TryTokenValidation(token, out var member);
            return Ok(member);
        }

        [HttpPost("Login")]
        public bool Login(LoginDto loginDto)
        {
            return userService.TryLogin(loginDto, out var jwt);
        }

        [HttpPost("Register")]
        public bool Register(RegisterDto dto)
        {
            return userService.TryRegister(dto, 2, out var dir);
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
        
        [HttpPost("makeDir")]
        public bool MakeDir(string id)
        {
            return fileService.TryMakeTemplateDirectory(id);
        }

        [HttpPost("getFiles")]
        public List<FileDto> getFiles(string id, string path)
        {
            return fileService.GetFiles(id, path);
        }

        [HttpPost("teapot")]
        public void teapot()
        {
            throw new HttpErrorException(new HttpErrorDetail()
            {
                ErrorCode = 418,
                Message = "I'm a teapot"
            });
        }

        [HttpPost("upload")]
        public async Task<bool> Upload(IFormFile file, string? path, [FromHeader]string auth)
        {
            if (!jwtService.TryTokenValidation(auth, out var memberDto)
                || memberDto is null)
            {
                return false;
            }
            return await fileService.UploadFile(file, memberDto, path); ;
        }

        [HttpPost("download")]
        public IActionResult Download(string path, [FromHeader] string auth)
        {
            if (!jwtService.TryTokenValidation(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            if (fileService.DownloadFile(memberDto, path, out var fileStream)
                || fileStream is null)
            {
                return StatusCode(404);
            }
            return new FileStreamResult(fileStream, "application/octet-stream") {
                FileDownloadName = Path.GetFileName(fileStream.Name), EnableRangeProcessing = true 
            };
        }

        [HttpPost("share")]
        public async Task<IActionResult> Share(ShareRequestDto req, [FromHeader] string auth)
        {
            if (!jwtService.TryTokenValidation(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            var token = await shareService.Share(memberDto, req);
            return Ok(token);
        }

        [HttpPost("get_shares")]
        public async Task<IActionResult> GetShares([FromHeader] string auth)
        {
            if (!jwtService.TryTokenValidation(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            var res = await shareService.GetSharesAsync(memberDto);
            return Ok(res);
        }

        [HttpPost("get_share")]
        public async Task<IActionResult> GetShare(string token)
        {
            var res = await shareService.GetShareAsync(token);
            return Ok(res);
        }

        [HttpPost("dl_share")]
        public async Task<IActionResult> DLShare(string token, string? password)
        {
            var fs = await shareService.DownloadShareAsync(token, password);
            return new FileStreamResult(fs, "application/octet-stream")
            {
                FileDownloadName = Path.GetFileName(fs.Name),
                EnableRangeProcessing = true
            };
        }

        [HttpPost("closeShare")]
        public async Task<IActionResult> CloseShare(string token, [FromHeader] string auth)
        {
            if (!jwtService.TryTokenValidation(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403);
            }
            var result = await shareService.CloseShareAsync(memberDto, token);
            return result ? Ok() : BadRequest();
        }
    }
}
