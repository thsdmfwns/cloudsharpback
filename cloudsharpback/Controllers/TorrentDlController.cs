using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TorrentDlController : ControllerBase
    {
        private readonly ITorrentDlService dlService;
        private readonly IJWTService jwtService;

        public TorrentDlController(ITorrentDlService dlService, IJWTService jwtService)
        {
            this.dlService = dlService;
            this.jwtService = jwtService;
        }

        [HttpPost("add_torrent")]
        public async Task<IActionResult> AddTorrent([FromHeader] string auth, string torrentPath, string? dlPath)
        {
            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403, "bad auth");
            }
            var result = await dlService.addTorrentAsync(memberDto, torrentPath, dlPath ?? string.Empty);
            if (result.err is not null)
            {
                return StatusCode(result.err.ErrorCode, result.err.Message);
            }

            return Ok(result.torrentHash);
        }

        [HttpPost("add_magnet")]
        public async Task<IActionResult> AddMagnet([FromHeader] string auth, string magnetUrl, string? dlPath)
        {
            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403, "bad auth");
            }
            var result = await dlService.addMagnetAsync(memberDto, magnetUrl, dlPath ?? string.Empty);
            if (result.err is not null)
            {
                return StatusCode(result.err.ErrorCode, result.err.Message);
            }
            return Ok(result.torrentHash);
        }

        [HttpGet("all")]
        public async Task<IActionResult> Getall([FromHeader] string auth)
        {
            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403, "bad auth");
            }
            return Ok(await dlService.GetAllAsync(memberDto));
        }

        [HttpPost("rm")]
        public async Task<IActionResult> Remove([FromHeader] string auth, string torrent_hash)
        {
            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403, "bad auth");
            }
            var err = await dlService.removeTorrent(memberDto, torrent_hash);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message); 
            }
            return Ok();
        }

        [HttpPost("run")]
        public async Task<IActionResult> Run([FromHeader] string auth, string torrent_hash)
        {
            if (!jwtService.TryValidateAcessToken(auth, out var memberDto)
                || memberDto is null)
            {
                return StatusCode(403, "bad auth");
            }
            var err = await dlService.StartTorrent(memberDto, torrent_hash);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }
    }
}
