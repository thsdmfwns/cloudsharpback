using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;

namespace cloudsharpback.Services.Interfaces
{
    public interface IYoutubeDlService
    {
        HttpResponseDto? Download(MemberDto member, string youtubeUrl, string? path, Guid requestToken);
        Task<bool> ValidateConnectionToken(string connId, string tokenString);
    }
}