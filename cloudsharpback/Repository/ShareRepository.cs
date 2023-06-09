using cloudsharpback.Models;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using Dapper;

namespace cloudsharpback.Repository;

public class ShareRepository : IShareRepository
{
    private readonly IDBConnService _connService;

    public ShareRepository(IDBConnService connService)
    {
        _connService = connService;
    }

    public async Task<bool> TryAddShare(ulong memberId, ShareRequestDto req, string? password, FileInfo fileinfo)
    {
        const string sql = "INSERT INTO share(member_id, target, password, expire_time, comment, share_time, share_name, token, file_size) " +
                           "VALUES(@MemberID, @Target, @Password, @ExpireTime, @Comment, @ShareTime, @ShareName, UUID_TO_BIN(@Token), @FileSize)";
        using var conn = _connService.Connection;
        var token = Guid.NewGuid().ToString();
        var res =await conn.ExecuteAsync(sql, new
        {
            MemberId = memberId,
            Target = req.Target,
            Password = password,
            ExpireTime = req.ExpireTime ?? (ulong)DateTime.MaxValue.Ticks,
            Comment = req.Comment,
            ShareTime = DateTime.UtcNow.Ticks,
            ShareName = req.ShareName,
            Token = token,
            FileSize = (ulong)fileinfo.Length,
        });
        return res > 0;
    }

    public async Task<ShareResponseDto?> GetShareByToken(string token)
    {
        //ulong id, ulong ownerId, string ownerNick, ulong shareTime, ulong? expireTime, string target, string? shareName, string? comment
        const string sql = "Select m.member_id ownerId, m.nickname ownerNick, " +
                           "s.share_time shareTime, s.expire_time expireTime, s.target target, " +
                           "s.share_name shareName, s.comment, BIN_TO_UUID(s.token) token, s.password, s.file_size filesize " +
                           "FROM share AS s " +
                           "INNER JOIN member AS m " +
                           "ON s.member_id = m.member_id " +
                           "WHERE s.token = UUID_TO_BIN(@Token)";
        using var conn = _connService.Connection;
        return await conn.QueryFirstOrDefaultAsync<ShareResponseDto>(sql, new { Token = token });
    }

    public async Task<List<ShareResponseDto>> GetSharesListByMemberId(ulong memberId)
    {
        const string sql = "Select m.member_id ownerId, m.nickname ownerNick, " +
                           "s.share_time shareTime, s.expire_time expireTime, s.target, " +
                           "s.share_name shareName, s.comment, BIN_TO_UUID(s.token) token, s.password, s.file_size filesize " +
                           "FROM share AS s " +
                           "INNER JOIN member AS m " +
                           "ON s.member_id = m.member_id " +
                           "WHERE s.member_id = @ID AND s.expire_time >= @Now";
        using var conn = _connService.Connection;
        var result = await conn.QueryAsync<ShareResponseDto>(sql, new { ID = memberId, Now = (ulong)DateTime.UtcNow.Ticks });
        return result.ToList();
    }

    public async Task<ShareDownloadDto?> GetShareDownloadDtoByToken(string token)
    {
        const string sql = "Select BIN_TO_UUID(m.directory) directory , s.target, s.expire_time expireTime, s.password " +
                           "FROM share AS s " +
                           "INNER JOIN member AS m " +
                           "ON s.member_id = m.member_id " +
                           "WHERE s.token = UUID_TO_BIN(@Token)";
        using var conn = _connService.Connection;
        return await conn.QueryFirstOrDefaultAsync<ShareDownloadDto?>(sql, new { Token = token });
    }

    public async Task<bool> TrySetShareExpireTimeToZero(ulong memberId, string token)
    {
        const string sql = "UPDATE share " +
                           "SET expire_time = 0 " +
                           "WHERE member_id = @Id AND token = UUID_TO_BIN(@Token)";
        using var conn = _connService.Connection;
        var result = await conn.ExecuteAsync(sql, new
        {
            Id = memberId,
            Token = token,
        });
        return result > 0;
    }

    public async Task<bool> TryUpdateShare(ulong memberId, string token, ShareUpdateDto dto, string? password)
    {
        const string sql = "UPDATE share " +
                           "SET password = @Password, expire_time = @Expire, comment = @Comment, share_name = @ShareName " +
                           "WHERE member_id = @Id AND token = UUID_TO_BIN(@Token)";
        using var conn = _connService.Connection;
        var res = await conn.ExecuteAsync(sql, new
        {
            Password = password,
            Expire = dto.ExpireTime ?? (ulong)DateTime.MaxValue.Ticks,
            Comment = dto.Comment,
            ShareName = dto.ShareName,
            Token = token,
            Id = memberId,
        });
        return res > 0;
    }

    public async Task<bool> TryDeleteShare(ulong memberId, string targetFilePath)
    {
        const string sql = "DELETE FROM share " +
                           "WHERE member_id = @Id AND target = @Target";
        using var conn = _connService.Connection;
        var res = await conn.ExecuteAsync(sql, new
        {
            Target = targetFilePath,
            Id = memberId,
        });
        return res > 0;
    }

    public async Task<string?> GetPasswordHashByToken(string token)
    {
        var sql = "Select password FROM share WHERE token = UUID_TO_BIN(@Token)";
        using var conn = _connService.Connection;
        return await conn.QueryFirstOrDefaultAsync<string>(sql, new
        {
            Token = token,
        });
    }
    
    
    
}