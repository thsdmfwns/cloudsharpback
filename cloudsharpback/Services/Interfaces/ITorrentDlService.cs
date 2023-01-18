using cloudsharpback.Models;

namespace cloudsharpback.Services.Interfaces
{
    public interface ITorrentDlService
    {
        Task<(HttpErrorDto? err, string? torrentHash)> addMagnetAsync(MemberDto member, string magnetUrl, string downloadPath);
        Task<(HttpErrorDto? err, string? torrentHash)> addTorrentAsync(MemberDto member, string torrentFilePath, string downloadPath);
        Task<List<TorrentInfoDto>> GetAllAsync(MemberDto member);
        ValueTask<HttpErrorDto?> removeTorrent(MemberDto member, string torrentHash);
        Task<HttpErrorDto?> StartTorrent(MemberDto member, string torrentHash);
    }
}