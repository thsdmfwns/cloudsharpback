using CliWrap;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YoutubeDlController : ControllerBase
    {
        private readonly IYoutubeDlService dlService;

        public YoutubeDlController(IYoutubeDlService dlService)
        {
            this.dlService = dlService;
        }

        [HttpPost("dl")]
        public IActionResult DownloadYoutube([FromHeader] string auth, [FromHeader] string req_token, string youtubeUrl, string? path)
        {
            if (!Guid.TryParse(req_token, out var requsetToken))
            {
                return StatusCode(400, "bad request token");
            }
            var err = dlService.Download(auth, youtubeUrl, path ?? string.Empty, requsetToken);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok(requsetToken.ToString());
        }
    }
}
