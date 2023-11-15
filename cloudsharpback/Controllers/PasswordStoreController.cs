using cloudsharpback.Controllers.Base;
using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.PasswordStore;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace cloudsharpback.Controllers;

[Route("api/Pass")]
[ApiController]
public class PasswordStoreController : AuthControllerBase
{
    private IPasswordStoreService _passwordStoreService;

    public PasswordStoreController(IPasswordStoreService passwordStoreService)
    {
        _passwordStoreService = passwordStoreService;
    }
    
    [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(List<PasswordStoreDirDto>))]
    [HttpGet("dir/ls")]
    public async Task<IActionResult> GetDirList()
    {
        return Ok(await _passwordStoreService.GetDirList(Member));
    }

    [SwaggerResponse(StatusCodes.Status200OK, "success")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "bad request")]
    [HttpPost("dir/new")]
    public async Task<IActionResult> MakeNewDIr(PasswordStoreDirInsertDto dto)
    {
        var err = await _passwordStoreService.MakeNewDir(Member, dto);
        return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok();
    }
    
    [SwaggerResponse(StatusCodes.Status200OK, "success")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "directory not found")]
    [HttpPost("dir/rm")]
    public async Task<IActionResult> RemoveDir(ulong itemId)
    {
        var err = await _passwordStoreService.RemoveDir(Member, itemId);
        return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok();
    }

    
    [SwaggerResponse(StatusCodes.Status200OK, "success")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "directory not found")]
    [HttpPost("dir/edit")]
    public async Task<IActionResult> UpdateDir([FromBody]PasswordStoreDirInsertDto dto, [FromQuery]ulong itemId)
    {
        var err = await _passwordStoreService.UpdateDir(Member, dto, itemId);
        return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok();
    }

    
    [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(PasswordStoreValueDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "value not found")]
    [HttpGet("val/{itemId}")]
    public async Task<IActionResult> GetValue(ulong itemId)
    {
        var result = await _passwordStoreService.GetValue(Member, itemId);
        return result.err is not null ? StatusCode(result.err.HttpCode, result.err.Message) : Ok(result.value);
    }
    
    
    [SwaggerResponse(StatusCodes.Status200OK, "success")]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
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
    

    [SwaggerResponse(StatusCodes.Status200OK, "success")]
    [SwaggerResponse(StatusCodes.Status404NotFound)]
    [HttpPost("val/new")]
    public async Task<IActionResult> MakeNewValues(PasswordStoreValueInsertDto dto)
    {
        var err = await _passwordStoreService.MakeNewValue(Member, dto);
        return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok();
    }
    
    [SwaggerResponse(StatusCodes.Status200OK, "success")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "value not found")]
    [HttpPost("val/rm")]
    public async Task<IActionResult> RemoveValue(ulong itemId)
    {
        var err = await _passwordStoreService.RemoveValue(Member, itemId);
        return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok();
    }
    
    [SwaggerResponse(StatusCodes.Status200OK, "success")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "value not found")]
    [HttpPost("val/edit")]
    public async Task<IActionResult> UpdateValue([FromQuery]ulong itemId, [FromBody]PasswordStoreValueUpdateDto dto)
    {
        var err = await _passwordStoreService.UpdateValue(Member, itemId, dto);
        return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok();
    }

    
    [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(PasswordStoreKeyDto))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "key not found")]
    [HttpGet("key/{itemId}")]
    public async Task<IActionResult> GetKey(ulong itemId)
    {
        var res = await _passwordStoreService.GetKey(Member, itemId);
        return res.err is not null ? StatusCode(res.err.HttpCode, res.err.Message) : Ok(res.value);
    }
    
    [SwaggerResponse(StatusCodes.Status200OK, "success", Type = typeof(List<PasswordStoreKeyDto>))]
    [HttpGet("key/ls")]
    public async Task<IActionResult> GetKeyList()
    {
        return Ok(await _passwordStoreService.GetKeyList(Member));
    }
    
    [SwaggerResponse(StatusCodes.Status200OK, "success")]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [HttpPost("key/new")]
    public async Task<IActionResult> MakeNewKey(PasswordStoreKeyInsertDto dto)
    {
        var err = await _passwordStoreService.MakeNewKey(Member, dto);
        return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok();
    }
    
    
    [SwaggerResponse(StatusCodes.Status200OK, "success")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "key not found")]
    [HttpPost("key/rm")]
    public async Task<IActionResult> RemoveKey(ulong itemId)
    {
        var err = await _passwordStoreService.RemoveKey(Member, itemId);
        return err is not null ? StatusCode(err.HttpCode, err.Message) : Ok();
    }
}