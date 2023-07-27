using System.Xml;
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
            if (!await _directoryRepository.InsertDir(memberDto.Id, dto))
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

    public async Task<(PasswordStoreValueDto? value, HttpResponseDto? err)> GetValue(MemberDto memberDto, ulong itemId)
    {
        try
        {
            var val = await _valueRepository.GetValueById(itemId);
            if (val is null)
            {
                return (null, new HttpResponseDto() { HttpCode = 404 });
            }
            if (! await CheckKeyAndDirIsMine(memberDto, val))
            {
                return (null, new HttpResponseDto() { HttpCode = 403 });
            }
            return (val, null);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            _logger.LogError(exception.StackTrace);
            throw new HttpErrorException(new HttpResponseDto()
            {
                HttpCode = 500,
                Message = "fail to GetValue"
            });
            
        }
    }

    public async Task<(List<PasswordStoreValueListItemDto> value, HttpResponseDto? err)> GetValuesList(MemberDto memberDto, ulong? keyId, ulong? dirId)
    {
        try
        {
            var empty = new List<PasswordStoreValueListItemDto>();
            if (!keyId.HasValue && dirId.HasValue)
            {
                var id = dirId.Value;
                if (!await CheckDirIsMine(memberDto, id))
                {
                    return (empty, new HttpResponseDto() { HttpCode = 403 });
                }
                return (await _valueRepository.GetValuesByDirectoryId(id), null);
            }

            if (keyId.HasValue && !dirId.HasValue)
            {
                var id = keyId.Value;
                if (!await CheckKeyIsMine(memberDto, id))
                {
                    return (empty, new HttpResponseDto() { HttpCode = 403 });
                }
                return (await _valueRepository.GetValuesByKeyId(id), null);
            }
        
            if (! await CheckKeyAndDirIsMine(memberDto, keyId!.Value, dirId!.Value))
            {
                return (empty, new HttpResponseDto() { HttpCode = 403 });
            }
            return (await _valueRepository.GetValuesByKeyIdAndDirId(dirId.Value, keyId.Value), null);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            _logger.LogError(exception.StackTrace);
            throw new HttpErrorException(new HttpResponseDto()
            {
                HttpCode = 500,
                Message = "fail to GetValuesList"
            });
        }
    }

    public async Task<HttpResponseDto?> MakeNewValue(MemberDto memberDto, PasswordStoreValueInsertDto dto)
    {
        try
        {
            if (! await CheckDirIsMine(memberDto, dto.DirectoryId)
                || ! await CheckKeyIsMine(memberDto, dto.KeyId))
            {
                return new HttpResponseDto() { HttpCode = 403 };
            }

            if (!await _valueRepository.InsertValue(dto.DirectoryId, dto.KeyId, dto.ValueId, dto.ValuePassword))
            {
                return new HttpResponseDto() { HttpCode = 400 };
            }
            return null;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            _logger.LogError(exception.StackTrace);
            throw new HttpErrorException(new HttpResponseDto()
            {
                HttpCode = 500,
                Message = "fail to MakeNewValue"
            });
        }
    }

    public async Task<HttpResponseDto?> RemoveValue(MemberDto memberDto, ulong itemId)
    {
        try
        {
            if (! await CheckValueIsMine(memberDto, itemId))
            {
                return new HttpResponseDto() { HttpCode = 403 };
            }

            if (!await _valueRepository.DeleteValue(itemId))
            {
                return new HttpResponseDto() { HttpCode = 404 };
            }

            return null;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            _logger.LogError(exception.StackTrace);
            throw new HttpErrorException(new HttpResponseDto()
            {
                HttpCode = 500,
                Message = "fail to RemoveValue"
            });
        }
    }

    public async Task<HttpResponseDto?> UpdateValue(MemberDto memberDto, ulong itemId, PasswordStoreValueUpdateDto dto)
    {
        try
        {
            if (! await CheckValueIsMine(memberDto, itemId))
            {
                return new HttpResponseDto() { HttpCode = 403 };
            }

            if (! await _valueRepository.UpdateValue(itemId, dto.ValueId, dto.ValuePassword))
            {
                return new HttpResponseDto() { HttpCode = 404 };
            }

            return null;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            _logger.LogError(exception.StackTrace);
            throw new HttpErrorException(new HttpResponseDto()
            {
                HttpCode = 500,
                Message = "fail to UpdateValue"
            });
        }
    }

    public async Task<(PasswordStoreKeyDto? value, HttpResponseDto? err)> GetKey(MemberDto memberDto, ulong itemId)
    {
        try
        {
            var res = await _keyRepository.GetKeyById(memberDto.Id, itemId);
            if (res is null)
            {
                return (null, new HttpResponseDto() { HttpCode = 404 });
            }

            return (res, null);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
            _logger.LogError(exception.StackTrace);
            throw new HttpErrorException(new HttpResponseDto()
            {
                HttpCode = 500,
                Message = "fail to GetKey"
            });
        }
        
    }

    public async Task<List<PasswordStoreKeyListItemDto>> GetKeyList(MemberDto memberDto)
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
                Message = "fail to GetKeyList",
            });
        }
    }

    public async Task<HttpResponseDto?> MakeNewKey(MemberDto memberDto, PasswordStoreKeyInsertDto dto)
    {
        try
        {
            if (!Enum.IsDefined(typeof(PasswordEncryptAlgorithm), dto.EncryptAlgorithm))
            {
                return new HttpResponseDto() { HttpCode = 400, Message = "Bad Encrypt Algorithm" };
            }
            if (!await _keyRepository.InsertKey(memberDto.Id, dto.EncryptAlgorithm, dto.PublicKey, dto.PrivateKey, dto.Name, dto.Comment))
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
                Message = "fail to MakeNewKey",
            });
        }
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
            throw new HttpErrorException(new HttpResponseDto
            {
                HttpCode = 500,
                Message = "fail to RemoveKey",
            });
            throw;
        }
    }

    private async Task<bool> CheckDirIsMine(MemberDto memberDto, ulong dirId)
    {
        var dir = await _directoryRepository.GetDirById(memberDto.Id, dirId);
        return dir is not null;
    }
    
    private async Task<bool> CheckKeyIsMine(MemberDto memberDto, ulong keyId)
    {
        var dir = await _keyRepository.GetKeyById(memberDto.Id, keyId);
        return dir is not null;
    }

    private async Task<bool> CheckKeyAndDirIsMine(MemberDto memberDto, ulong keyId, ulong dirId)
        => await CheckDirIsMine(memberDto, dirId) && await CheckKeyIsMine(memberDto, keyId);

    private async Task<bool> CheckKeyAndDirIsMine(MemberDto memberDto, PasswordStoreValueDto dto)
        => await CheckKeyAndDirIsMine(memberDto, dto.KeyId, dto.DirectoryId);
    private async Task<bool> CheckValueIsMine(MemberDto memberDto, ulong itemId)
    {
        var val = await _valueRepository.GetValueById(itemId);
        return val is not null &&
               await CheckDirIsMine(memberDto, val.DirectoryId) &&
               await CheckKeyIsMine(memberDto, val.KeyId);
    }
}