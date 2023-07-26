using cloudsharpback.Models;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;

namespace cloudsharpback.Services;

public class PasswordStoreService : IPasswordStoreService
{
    private readonly IPasswordStoreDirectoryRepository _directoryRepository;
    private readonly IPasswordStoreValueRepository _valueRepository;
    private readonly IPasswordStoreKeyRepository _keyRepository;
    private readonly ILogger _logger;

    public PasswordStoreService(IPasswordStoreDirectoryRepository directoryRepository, ILogger<IPasswordStoreService> logger, IPasswordStoreValueRepository valueRepository, IPasswordStoreKeyRepository keyRepository)
    {
        _directoryRepository = directoryRepository;
        _logger = logger;
        _valueRepository = valueRepository;
        _keyRepository = keyRepository;
    }


    public async Task<List<PasswordStoreDirDto>> GetDirList(MemberDto memberDto)
    {
        try
        {
            return await _directoryRepository.GetDirListByMemberId(memberDto.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            _logger.LogError(exception.StackTrace);
            throw new HttpErrorException(new HttpResponseDto
            {
                HttpCode = 500,
                Message = "fail to GetDirList",
            });
        }
    }

    public async Task<HttpResponseDto?> MakeNewDir(MemberDto memberDto, PasswordStoreDirInsertDto dto)
    {
        try
        {
            if (!await _directoryRepository.InstertDir(memberDto.Id, dto.Name, dto.Comment, dto.Icon))
            {
                return new HttpResponseDto() { HttpCode = 400 };
            }
            return null;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            _logger.LogError(exception.StackTrace);
            throw new HttpErrorException(new HttpResponseDto
            {
                HttpCode = 500,
                Message = "fail to MakeNewDir",
            });
            throw;
        }
    }

    public async Task<HttpResponseDto?> RemoveDir(MemberDto memberDto, ulong id)
    {
        try
        {
            if (!await  _directoryRepository.DeleteDir(memberDto.Id, id))
            {
                return new HttpResponseDto() { HttpCode = 404 };
            }
            return null;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            _logger.LogError(exception.StackTrace);
            throw new HttpErrorException(new HttpResponseDto
            {
                HttpCode = 500,
                Message = "fail to RemoveDir",
            });
        }
    }

    public async Task<HttpResponseDto?> UpdateDir(MemberDto memberDto, PasswordStoreDirInsertDto dto, ulong itemId)
    {
        try
        {
            if (!await  _directoryRepository.UpdateDir(memberDto.Id, itemId, dto.Name, dto.Comment, dto.Icon))
            {
                return new HttpResponseDto() { HttpCode = 404 };
            }

            return null;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            _logger.LogError(exception.StackTrace);
            throw new HttpErrorException(new HttpResponseDto
            {
                HttpCode = 500,
                Message = "fail to UpdateDir",
            });
        }
    }

    public async Task<(List<PasswordStoreValueDto> value, HttpResponseDto? err)> GetValuesList(MemberDto memberDto, ulong? keyId, ulong? dirId)
    {
        var empty = new List<PasswordStoreValueDto>();
        if (!keyId.HasValue && dirId.HasValue)
        {
            var id = dirId.Value;
            if (!await CheckDirIsMine(memberDto, id))
            {
                return (empty, new HttpResponseDto() { HttpCode = 403 });
            }
            return (await _valueRepository.GetPasswordStoreValuesByDirectoryId(id), null);
        }

        if (keyId.HasValue && !dirId.HasValue)
        {
            var id = keyId.Value;
            if (!await CheckKeyIsMine(memberDto, id))
            {
                return (empty, new HttpResponseDto() { HttpCode = 403 });
            }
            return (await _valueRepository.GetPasswordStoreValuesByKeyId(id), null);
        }
        
        if (! await CheckDirIsMine(memberDto, dirId!.Value)
            || ! await CheckKeyIsMine(memberDto, keyId!.Value))
        {
            return (empty, new HttpResponseDto() { HttpCode = 403 });
        }
        return (await _valueRepository.GetPasswordStoreValuesByKeyIdAndDirId(dirId.Value, keyId.Value), null);
    }

    public async Task<List<PasswordStoreKeyDto>> GetKeyList(MemberDto memberDto)
    {
        try
        {
            return await _keyRepository.GetKeyListByMemberId(memberDto.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            _logger.LogError(exception.StackTrace);
            throw new HttpErrorException(new HttpResponseDto
            {
                HttpCode = 500,
                Message = "fail to GetDirList",
            });
        }
    }

    public async Task<HttpResponseDto?> MakeNewKey(MemberDto memberDto, PasswordStoreKeyInsertDto dto)
    {
        if (!Enum.IsDefined(typeof(PasswordEncryptAlgorithm), dto.EncryptAlgorithm))
        {
            return new HttpResponseDto() { HttpCode = 400, Message = "Bad Encrypt Algorithm" };
        }
        if (!await _keyRepository.InsertKey(memberDto.Id, dto.EncryptAlgorithm, dto.PublicKey, dto.PrivateKey))
        {
            return new HttpResponseDto() { HttpCode = 400 };
        }
        
        return null;
    }

    public async Task<HttpResponseDto?> RemoveKey(MemberDto memberDto, ulong itemId)
    {
        try
        {
            if (!await _keyRepository.DeleteKeyById(memberDto.Id, itemId))
            {
                return new HttpResponseDto() { HttpCode = 404, Message = "Key Not Found" };
            }

            return null;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            _logger.LogError(exception.StackTrace);
            throw;
        }
    }

    private async Task<bool> CheckDirIsMine(MemberDto memberDto, ulong dirId)
    {
        var dir = await _directoryRepository.GetDirById(dirId);
        return dir is not null && dir.OwnerId == memberDto.Id;
    }
    
    private async Task<bool> CheckKeyIsMine(MemberDto memberDto, ulong keyId)
    {
        var dir = await _keyRepository.GetKeyById(keyId);
        return dir is not null && dir.OwnerId == memberDto.Id;
    }
}