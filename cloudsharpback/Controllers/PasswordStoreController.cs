using cloudsharpback.Controllers.Base;
using cloudsharpback.Models;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace cloudsharpback.Controllers;

[Route("api/pass")]
[ApiController]
public class PasswordStoreController : AuthControllerBase
{
    private IPasswordStoreService _passwordStoreService;

    public PasswordStoreController(IPasswordStoreService passwordStoreService)
    {
        _passwordStoreService = passwordStoreService;
    }

    [HttpGet("dir/ls")]
    public async Task<IActionResult> GetDirList()
    {
        return Ok(await _passwordStoreService.GetDirList(Member));
    }

    [HttpPost("dir/new")]
    public async Task<IActionResult> MakeNewDIr(PasswordStoreDirInsertDto dto)
    {
        var err = await _passwordStoreService.MakeNewDir(Member, dto);
        if (err is not null)
        {
            return StatusCode(err.HttpCode, err.Message);
        }
        return Ok();
    }
    
    [HttpPost("dir/rm")]
    public async Task<IActionResult> RemoveDir(ulong itemId)
    {
        var err = await _passwordStoreService.RemoveDir(Member, itemId);
        if (err is not null)
        {
            return StatusCode(err.HttpCode, err.Message);
        }
        return Ok();
    }

    [HttpPost("dir/edit")]
    public async Task<IActionResult> UpdateDir([FromBody]PasswordStoreDirInsertDto dto, [FromQuery]ulong itemId)
    {
        var err = await _passwordStoreService.UpdateDir(Member, dto, itemId);
        if (err is not null)
        {
            return StatusCode(err.HttpCode, err.Message);
        }
        return Ok();
    }
    
    
    [HttpGet("val/ls")]
    public async Task<IActionResult> GetValues(ulong? directoryId, ulong? keyId)
    {
        if (!directoryId.HasValue && !keyId.HasValue)
        {
            return BadRequest();
        }
        var result = await _passwordStoreService.GetValuesList(Member, keyId, directoryId);
        return result.err is not null ? StatusCode(result.err.HttpCode, result.err.Message) : Ok(result.value);
    }
    

    [HttpPost("val/new")]
    public async Task<IActionResult> MakeNewValues(PasswordStoreValueInsertDto dto)
    {
        var err = await _passwordStoreService.MakeNewValue(Member, dto);
        if (err is not null)
        {
            return StatusCode(err.HttpCode, err.Message);
        }
        return Ok();
    }
    
    [HttpPost("val/rm")]
    public async Task<IActionResult> RemoveValue(ulong itemId)
    {
        var err = await _passwordStoreService.RemoveValue(Member, itemId);
        if (err is not null)
        {
            return StatusCode(err.HttpCode, err.Message);
        }
        return Ok();
    }
    
    [HttpPost("val/edit")]
    public async Task<IActionResult> UpdateValue([FromQuery]ulong itemId, [FromBody]PasswordStoreValueUpdateDto dto)
    {
        var err = await _passwordStoreService.UpdateValue(Member, itemId, dto);
        if (err is not null)
        {
            return StatusCode(err.HttpCode, err.Message);
        }
        return Ok();
    }
    
    [HttpGet("key/ls")]
    public async Task<IActionResult> GetKeyList()
    {
        return Ok(await _passwordStoreService.GetKeyList(Member));
    }

    [HttpPost("key/new")]
    public async Task<IActionResult> MakeNewKey(PasswordStoreKeyInsertDto dto)
    {
        var err = await _passwordStoreService.MakeNewKey(Member, dto);
        if (err is not null)
        {
            return StatusCode(err.HttpCode, err.Message);
        }
        return Ok();
    }
    
    [HttpPost("key/rm")]
    public async Task<IActionResult> RemoveKey(ulong itemId)
    {
        var err = await _passwordStoreService.RemoveKey(Member, itemId);
        if (err is not null)
        {
            return StatusCode(err.HttpCode, err.Message);
        }
        return Ok();
    }
}