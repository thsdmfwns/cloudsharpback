using cloudsharpback.Models;

namespace cloudsharpback.Repository.Interface;

public interface IShareRepository
{
    Task<bool> TryAddShare(ulong memberId, ShareRequestDto req, string? password, FileInfo fileinfo);
    Task<ShareResponseDto?> GetShareByToken(string token);
    Task<List<ShareResponseDto>> GetSharesListByMemberId(ulong memberId);
    Task<ShareDownloadDto?> GetShareDownloadDtoByToken(string token);
    Task<bool> TrySetShareExpireTimeToZero(ulong memberId, string token);
    Task<bool> TryUpdateShare(ulong memberId, string token, ShareUpdateDto dto, string? password);
    Task<bool> TryDeleteShare(ulong memberId, string targetFilePath);
    Task<string?> GetPasswordHashByToken(string token);
}