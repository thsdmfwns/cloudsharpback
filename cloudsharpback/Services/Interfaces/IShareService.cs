using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IShareService
    {
        Task<bool> CloseShareAsync(MemberDto member, string token);
        Task DeleteShareAsync(string target, MemberDto member);
        Task<(ServiceResult response, FileStream? result)> DownloadShareAsync(string token, string? password);
        Task<(ServiceResult response, ShareResponseDto? result)> GetShareAsync(string token);
        Task<List<ShareResponseDto>> GetSharesAsync(MemberDto member);
        Task<ServiceResult> Share(MemberDto member, ShareRequestDto req);
        Task<bool> UpdateShareAsync(ShareUpdateDto dto, string token, MemberDto member);
    }
}