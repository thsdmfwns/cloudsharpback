using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IShareService
    {
        Task<bool> CheckPassword(string token);
        Task<bool> CloseShareAsync(MemberDto member, string token);
        Task DeleteShareAsync(string target, MemberDto member);
        /// <returns>404 : share doesnt exist , 403 : bad password, 410 : expired share</returns>
        Task<(HttpErrorDto? err, Ticket? dlToken)> GetDownloadTokenAsync(ShareDowonloadRequestDto req, string reqIp);
        /// <returns>410 : expired share </returns>
        Task<(HttpErrorDto? err, ShareResponseDto? result)> GetShareAsync(string token);
        Task<List<ShareResponseDto>> GetSharesAsync(MemberDto member);
        /// <returns>404 : no file for share</returns>
        Task<HttpErrorDto?> Share(MemberDto member, ShareRequestDto req);
        Task<bool> UpdateShareAsync(ShareUpdateDto dto, string token, MemberDto member);
        /// <returns>404 : NotFound Share</returns>
        Task<(HttpErrorDto? err, bool? result)> ValidatePassword(string password, string token);
    }
}