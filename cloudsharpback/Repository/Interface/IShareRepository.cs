using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Share;

namespace cloudsharpback.Repository.Interface;

public interface IShareRepository
{
    Task<bool> TryAddShare(ulong memberId, ShareRequestDto req, string? password, FileInfo fileinfo);
    Task<bool> TryAddShare(ulong memberId, string target, string? password, ulong expireTime,
        string? comment,
        string? shareName, Guid token, ulong fileSize);
    Task<ShareResponseDto?> GetShareByToken(Guid token);
    Task<List<ShareResponseDto>> GetSharesByTargetFilePath(ulong memberid, string targetFilePath);
    Task<List<ShareResponseDto>> GetSharesListByMemberId(ulong memberId);
    Task<ShareDownloadDto?> GetShareDownloadDtoByToken(Guid token);
    Task<bool> TrySetShareExpireTimeToZero(ulong memberId, Guid token);
    Task<bool> TryUpdateShare(ulong memberId, Guid token, string? password, string? comment,
        ulong? expireTime, string? shareName);
    Task<bool> TryDeleteShare(ulong memberId, string targetFilePath);
    Task<int> TryDeleteShareInDirectory(ulong memberId, string targetDirectoryPath);
    Task<List<ShareResponseDto>> GetSharesInDirectory(ulong memberid, string targetDirectoryPath);
    Task<string?> GetPasswordHashByToken(Guid token);
}