using cloudsharpback.Controllers.Base;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YoutubeDlController : AuthControllerBase
    {
        private readonly IYoutubeDlService _dlService;

        public YoutubeDlController(IYoutubeDlService dlService)
        {
            this._dlService = dlService;
        }

        [HttpPost("dl")]
        public IActionResult DownloadYoutube([FromHeader] string reqToken, string youtubeUrl, string? path)
        {
            if (!Guid.TryParse(reqToken, out var requestToken))
            {
                return StatusCode(400, "bad request token");
            }
            var err = _dlService.Download(Member, youtubeUrl, path ?? string.Empty, requestToken);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok(requestToken.ToString());
        }
    }
}
