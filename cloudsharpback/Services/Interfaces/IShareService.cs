using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IShareService
    {
        Task<bool> CheckPassword(string token);
        Task<HttpResponseDto?> CloseShareAsync(MemberDto member, string token);
        Task<HttpResponseDto?> DeleteShareAsync(string target, MemberDto member);
        /// <returns>404 : share doesnt exist , 403 : bad password, 410 : expired share</returns>
        Task<(HttpResponseDto? err, ShareDownloadDto? dto)> GetDownloadDtoAsync(ShareDowonloadRequestDto req);
        /// <returns>410 : expired share </returns>
        Task<(HttpResponseDto? err, ShareResponseDto? result)> GetShareAsync(string token);
        Task<List<ShareResponseDto>> GetSharesAsync(MemberDto member);
        /// <returns>404 : no file for share</returns>
        Task<HttpResponseDto?> Share(MemberDto member, ShareRequestDto req);
        Task<HttpResponseDto?> UpdateShareAsync(ShareUpdateDto dto, string token, MemberDto member);
        /// <returns>404 : NotFound Share</returns>
        Task<(HttpResponseDto? err, bool? result)> ValidatePassword(string password, string token);
    }
}