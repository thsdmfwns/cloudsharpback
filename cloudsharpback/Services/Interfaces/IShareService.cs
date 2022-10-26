using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IShareService
    {
        Task<ShareResponseDto?> GetShareAsync(string token);
        Task<List<ShareResponseDto>> GetSharesAsync(MemberDto member);
        Task<string?> Share(MemberDto member, ShareRequestDto req);
        Task<FileStream> DownloadShareAsync(string token, string? password);
        Task<bool> CloseShareAsync(MemberDto member, string token);
    }
}