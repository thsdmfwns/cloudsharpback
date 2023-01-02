﻿using CliWrap;
using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Diagnostics.Metrics;
using System.IO;
using Transmission.API.RPC;
using Transmission.API.RPC.Entity;

namespace cloudsharpback.Services
{
    public class TorrentDlService : ITorrentDlService
    {
        private string DirectoryPath;
        private readonly ILogger _logger;
        private readonly IDBConnService _connService;

        public TorrentDlService(IConfiguration configuration, ILogger<ITorrentDlService> logger, IDBConnService connService)
        {
            DirectoryPath = configuration["File:DirectoryPath"];
            _logger = logger;
            _connService = connService;
        }

        string userPath(string directoryId) => Path.Combine(DirectoryPath, directoryId);
        async Task<TorrentInfo?> FindTorrentByHash(string hash) 
            => (await client.TorrentGetAsync(TorrentFields.ALL_FIELDS)).Torrents.ToList().FirstOrDefault(x => x.HashString == hash);
        async Task<List<TorrentInfo>> FindTorrentByHash(List<string> fields, List<string> hashes)
            => (await client.TorrentGetAsync(fields.ToArray())).Torrents.ToList().Where(x => hashes.Contains(x.HashString)).ToList();
        private Client client => new Client("http://127.0.0.1:9091/transmission/rpc", "", login: "transmission", password: "transmission");

        public async Task<(HttpErrorDto? err, string? torrentHash)> addTorrentAsync(MemberDto member, string torrentFilePath, string downloadPath)
        {
            try
            {
                var userDir = userPath(member.Directory);
                var filepath = Path.Combine(userDir, torrentFilePath);
                if (!File.Exists(filepath)
                    || Path.GetExtension(filepath) != ".torrent")
                {
                    var err = new HttpErrorDto()
                    {
                        ErrorCode = 404,
                        Message = "torrent not found"
                    };
                    return (err, null);
                }
                var dlDir = Path.Combine(userDir, downloadPath);
                var torrent = new NewTorrent()
                {
                    DownloadDirectory = dlDir,
                    Filename = filepath,
                };
                var torrentInfo = await client.TorrentAddAsync(torrent);
                var sql = "INSERT INTO torrent(owner_member_id, torrent_hash, download_path) " +
                        "VALUES(@memberId, @torrent_hash, @download_path)";
                using var conn = _connService.Connection;
                await conn.ExecuteAsync(sql, new
                {
                    memberId = member.Id,
                    torrent_hash = torrentInfo.HashString,
                    download_path = downloadPath,
                });
                return (null, torrentInfo.HashString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to add Torrent",
                });
            }
        }

        public async Task<(HttpErrorDto? err, string? torrentHash)> addMagnetAsync(MemberDto member, string magnetUrl, string downloadPath)
        {
            try
            {
                var userDir = userPath(member.Directory);
                if (!magnetUrl.StartsWith("magnet:"))
                {
                    var err = new HttpErrorDto()
                    {
                        ErrorCode = 400,
                        Message = "bad magnet"
                    };
                    return (err, null);
                }
                var dlDir = Path.Combine(userDir, downloadPath);
                var torrent = new NewTorrent()
                {
                    DownloadDirectory = dlDir,
                    Filename = magnetUrl,
                };
                var torrentInfo = await client.TorrentAddAsync(torrent);
                var sql = "INSERT INTO torrent(owner_member_id, torrent_hash, download_path) " +
                        "VALUES(@memberId, @torrent_hash, @download_path)";
                using var conn = _connService.Connection;
                await conn.ExecuteAsync(sql, new
                {
                    memberId = member.Id,
                    torrent_hash = torrentInfo.HashString,
                    download_path = downloadPath,
                });
                return (null, torrentInfo.HashString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to add Torrent",
                });
            }
        }

        public async Task<List<TorrentInfoDto>> GetAllAsync(MemberDto member)
        {
            try
            {
                var sql = "SELECT torrent_hash " +
                    "FROM torrent " +
                    "WHERE owner_member_id = @memberId";
                using var conn = _connService.Connection;
                var hashes = await conn.QueryAsync<string>(sql, new
                {
                    memberId = member.Id
                });
                if (hashes.Count() <= 0)
                {
                    return new();
                }
                var fields = new List<string>()
                {
                    TorrentFields.ID,
                    TorrentFields.NAME,
                    TorrentFields.TOTAL_SIZE,
                    TorrentFields.PERCENT_DONE,
                    TorrentFields.PEERS_CONNECTED,
                    TorrentFields.RATE_DOWNLOAD,
                    TorrentFields.RATE_UPLOAD,
                    TorrentFields.ERROR,
                    TorrentFields.ERROR_STRING,
                    TorrentFields.ETA,
                    TorrentFields.HASH_STRING,
                };
                return (await FindTorrentByHash(fields, hashes.ToList())).Select(TorrentInfoDto.FromTransMission).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to get Torrent",
                });
            }
        }

        public async ValueTask<HttpErrorDto?> removeTorrent(MemberDto member, string hash)
        {
            try
            {
                var sql = "SELECT torrent_id " +
                "FROM torrent " +
                "WHERE owner_member_id = @memberId AND torrent_hash = @torrentHash";
                var rmSql = "DELETE FROM torrent WHERE torrent_hash = @torrentHash";
                using var conn = _connService.Connection;
                var info = await FindTorrentByHash(hash);
                if (!(await conn.QueryAsync<long>(sql, new { memberId = member.Id, torrentHash = hash})).Any()
                    || info is null)
                {
                    return new HttpErrorDto
                    {
                        ErrorCode = 404,
                        Message = $"torrent:{hash} is not found",
                    };
                }
                var id = new int[] { info.ID };
                client.TorrentRemove(id);
                await conn.ExecuteAsync(rmSql, new { torrentHash = hash });
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to remove Torrent",
                });
            }
        }

        public async Task<HttpErrorDto?> StartTorrent(MemberDto member, string hash)
        {
            var sql = "SELECT torrent_id " +
            "FROM torrent " +
            "WHERE owner_member_id = @memberId AND torrent_hash = @torrentHash";
            using var conn = _connService.Connection;
            var info = await FindTorrentByHash(hash);
            if (!(await conn.QueryAsync<long>(sql, new { memberId = member.Id, torrentHash = hash })).Any()
                || info is null)
            {
                return new HttpErrorDto
                {
                    ErrorCode = 404,
                    Message = $"torrent:{hash} is not found",
                };
            }
            var input = new object[] { info.ID };
            client.TorrentStartNowAsync(input);
            return null;
        }
    }
}