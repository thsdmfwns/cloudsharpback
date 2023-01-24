using cloudsharpback.Controllers.Base;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TorrentDlController : AuthControllerBase
    {
        private readonly ITorrentDlService _dlService;

        public TorrentDlController(ITorrentDlService dlService)
        {
            this._dlService = dlService;
        }

        [HttpPost("add_torrent")]
        public async Task<IActionResult> AddTorrent(string torrentPath, string? dlPath)
        {
            var result = await _dlService.addTorrentAsync(Member!, torrentPath, dlPath ?? string.Empty);
            if (result.err is not null)
            {
                return StatusCode(result.err.ErrorCode, result.err.Message);
            }

            return Ok(result.torrentHash);
        }

        [HttpPost("add_magnet")]
        public async Task<IActionResult> AddMagnet(string magnetUrl, string? dlPath)
        {
            var result = await _dlService.addMagnetAsync(Member!, magnetUrl, dlPath ?? string.Empty);
            if (result.err is not null)
            {
                return StatusCode(result.err.ErrorCode, result.err.Message);
            }
            return Ok(result.torrentHash);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _dlService.GetAllAsync(Member!));
        }

        [HttpPost("rm")]
        public async Task<IActionResult> Remove(string torrent_hash)
        {
            var err = await _dlService.removeTorrent(Member!, torrent_hash);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message); 
            }
            return Ok();
        }

        [HttpPost("run")]
        public async Task<IActionResult> Run(string torrent_hash)
        {
            var err = await _dlService.StartTorrent(Member!, torrent_hash);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }
    }
}
