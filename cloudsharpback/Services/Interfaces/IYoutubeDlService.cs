using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface IYoutubeDlService
    {
        HttpErrorDto? Download(string auth, string youtubeUrl, string path, Guid requsestToken);
        Task OnSignalrConnected(string connId, string auth);
    }
}