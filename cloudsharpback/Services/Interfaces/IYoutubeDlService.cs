using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IYoutubeDlService
    {
        HttpErrorDto? Download(MemberDto member, string youtubeUrl, string path, Guid requestToken);
        Task OnSignalrConnected(string connId, string auth);
    }
}