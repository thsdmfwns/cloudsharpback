using cloudsharpback.Controllers.Base;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TorrentDlController : AuthControllerBase
    {
        private readonly ITorrentDlService _torrentDlService;

        public TorrentDlController(ITorrentDlService torrentDlService)
        {
            _torrentDlService = torrentDlService;
        }

        [HttpPost("addTorrent")]
        public async Task<IActionResult> AddTorrent(string torrentPath, string? dlPath)
        {
            var result = await _torrentDlService.addTorrentAsync(Member!, torrentPath, dlPath ?? string.Empty);
            if (result.err is not null)
            {
                return StatusCode(result.err.ErrorCode, result.err.Message);
            }

            return Ok(result.torrentHash);
        }

        [HttpPost("addMagnet")]
        public async Task<IActionResult> AddMagnet(string magnetUrl, string? dlPath)
        {
            var result = await _torrentDlService.addMagnetAsync(Member!, magnetUrl, dlPath ?? string.Empty);
            if (result.err is not null)
            {
                return StatusCode(result.err.ErrorCode, result.err.Message);
            }
            return Ok(result.torrentHash);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _torrentDlService.GetAllAsync(Member!));
        }

        [HttpPost("rm")]
        public async Task<IActionResult> Remove(string torrent_hash)
        {
            var err = await _torrentDlService.removeTorrent(Member!, torrent_hash);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message); 
            }
            return Ok();
        }

        [HttpPost("run")]
        public async Task<IActionResult> Run(string torrent_hash)
        {
            var err = await _torrentDlService.StartTorrent(Member!, torrent_hash);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }
    }
}
