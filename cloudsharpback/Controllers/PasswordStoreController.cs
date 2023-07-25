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
    public IActionResult MakeNewDIr(ulong id)
    {
        //todo dir 삭제
        return Ok();
    }

    [HttpPost("dir/re")]
    public IActionResult UpdateDir()
    {
        //todo dir 업데이트
        return Ok();
    }
    
    
    [HttpGet("val/ls")]
    public IActionResult GetValues(ulong? directoryId, ulong? keyId)
    {
        //todo 비밀번호 조회 
        return Ok(directoryId + keyId);
    }

    [HttpPost("val/new")]
    public IActionResult MakeNewValues()
    {
        //todo val 생성

        return Ok();
    }
    
    [HttpPost("val/rm")]
    public IActionResult RemoveValues()
    {
        //todo val 삭제

        return Ok();
    }
    
    [HttpPost("val/re")]
    public IActionResult UpdateValues()
    {
        //todo val 업데이트
        return Ok();
    }
    
    [HttpGet("key/ls")]
    public IActionResult GetKey(ulong? directoryId, ulong? keyId)
    {
        //todo 비밀번호 조회 
        return Ok(directoryId + keyId);
    }

    [HttpPost("key/new")]
    public IActionResult MakeNewKey()
    {
        //todo val 생성

        return Ok();
    }
    
    [HttpPost("key/rm")]
    public IActionResult RemoveKey()
    {
        //todo val 삭제

        return Ok();
    }
    
    [HttpPost("key/re")]
    public IActionResult UpdateKey()
    {
        //todo val 업데이트
        return Ok();
    }
}