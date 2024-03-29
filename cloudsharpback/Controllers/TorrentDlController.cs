﻿using cloudsharpback.Controllers.Base;
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
            var result = await _torrentDlService.addTorrentAsync(Member, torrentPath, dlPath ?? string.Empty);
            if (result.err is not null)
            {
                return StatusCode(result.err.HttpCode, result.err.Message);
            }

            return Ok(result.torrentHash);
        }

        [HttpPost("addMagnet")]
        public async Task<IActionResult> AddMagnet(string magnetUrl, string? dlPath)
        {
            var result = await _torrentDlService.addMagnetAsync(Member, magnetUrl, dlPath ?? string.Empty);
            if (result.err is not null)
            {
                return StatusCode(result.err.HttpCode, result.err.Message);
            }
            return Ok(result.torrentHash);
        }

        [HttpGet("ls")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _torrentDlService.GetAllAsync(Member));
        }

        [HttpPost("rm")]
        public async Task<IActionResult> Remove(string torrentHash)
        {
            var err = await _torrentDlService.removeTorrent(Member, torrentHash);
            if (err is not null)
            {
                return StatusCode(err.HttpCode, err.Message); 
            }
            return Ok();
        }

        [HttpPost("run")]
        public async Task<IActionResult> Run(string torrentHash)
        {
            var err = await _torrentDlService.StartTorrent(Member, torrentHash);
            if (err is not null)
            {
                return StatusCode(err.HttpCode, err.Message);
            }
            return Ok();
        }
    }
}
