using cloudsharpback.Models;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;

namespace cloudsharpback.Services;

public class PasswordStoreService : IPasswordStoreService
{
    private readonly IPasswordStoreDirectoryRepository _directoryRepository;
    private readonly ILogger _logger;

    public PasswordStoreService(IPasswordStoreDirectoryRepository directoryRepository, ILogger<IPasswordStoreService> logger)
    {
        _directoryRepository = directoryRepository;
        _logger = logger;
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
}