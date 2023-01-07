using cloudsharpback.Controllers.Base;
using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;

namespace cloudsharpback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TorrentDlController : AuthControllerBase
    {
        private readonly ITorrentDlService dlService;

        public TorrentDlController(ITorrentDlService dlService)
        {
            this.dlService = dlService;
        }

        [HttpPost("add_torrent")]
        public async Task<IActionResult> AddTorrent(string torrentPath, string? dlPath)
        {
            var result = await dlService.addTorrentAsync(Member!, torrentPath, dlPath ?? string.Empty);
            if (result.err is not null)
            {
                return StatusCode(result.err.ErrorCode, result.err.Message);
            }

            return Ok(result.torrentHash);
        }

        [HttpPost("add_magnet")]
        public async Task<IActionResult> AddMagnet(string magnetUrl, string? dlPath)
        {
            var result = await dlService.addMagnetAsync(Member!, magnetUrl, dlPath ?? string.Empty);
            if (result.err is not null)
            {
                return StatusCode(result.err.ErrorCode, result.err.Message);
            }
            return Ok(result.torrentHash);
        }

        [HttpGet("all")]
        public async Task<IActionResult> Getall()
        {
            return Ok(await dlService.GetAllAsync(Member!));
        }

        [HttpPost("rm")]
        public async Task<IActionResult> Remove(string torrent_hash)
        {
            var err = await dlService.removeTorrent(Member!, torrent_hash);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message); 
            }
            return Ok();
        }

        [HttpPost("run")]
        public async Task<IActionResult> Run(string torrent_hash)
        {
            var err = await dlService.StartTorrent(Member!, torrent_hash);
            if (err is not null)
            {
                return StatusCode(err.ErrorCode, err.Message);
            }
            return Ok();
        }
    }
}
