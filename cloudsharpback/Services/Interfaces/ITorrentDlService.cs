﻿using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.FIle;
using cloudsharpback.Models.DTO.Member;

namespace cloudsharpback.Services.Interfaces
{
    public interface ITorrentDlService
    {
        Task<(HttpResponseDto? err, string? torrentHash)> addMagnetAsync(MemberDto member, string magnetUrl, string downloadPath);
        Task<(HttpResponseDto? err, string? torrentHash)> addTorrentAsync(MemberDto member, string torrentFilePath, string downloadPath);
        Task<List<TorrentInfoDto>> GetAllAsync(MemberDto member);
        ValueTask<HttpResponseDto?> removeTorrent(MemberDto member, string torrentHash);
        Task<HttpResponseDto?> StartTorrent(MemberDto member, string torrentHash);
    }
}