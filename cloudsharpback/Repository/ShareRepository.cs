using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Share;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using Dapper;
using MySql.Data.MySqlClient;

namespace cloudsharpback.Repository;

public class ShareRepository : IShareRepository
{
    private readonly IDBConnectionFactory _connService;

    public ShareRepository(IDBConnectionFactory connService)
    {
        _connService = connService;
    }

    public async Task<bool> TryAddShare(ulong memberId, ShareRequestDto req, string? password, FileInfo fileinfo)
        => await TryAddShare(
            memberId: memberId,
            target: req.Target,
            password: password,
            expireTime: req.ExpireTime ?? (ulong)DateTime.MaxValue.Ticks,
            comment: req.Comment,
            shareName: req.ShareName,
            token: Guid.NewGuid(),
            fileSize: (ulong)fileinfo.Length
        );

    public async Task<bool> TryAddShare(ulong memberId, string target, string? password, ulong expireTime,
        string? comment,
        string? shareName, Guid token, ulong fileSize)
    {
        try
        {
            return await AddShare(memberId, target, password, expireTime, comment, shareName, token, fileSize);
        }
        catch (MySqlException ex)
        {
            if (ex.Number  == 1452 
                || ex.Number == 1451
                || ex.Number == 1062)
            {
                return false;
            }
            throw;
        }
    }

    private async Task<bool> AddShare(ulong memberId, string target, string? password, ulong expireTime, string? comment,
        string? shareName, Guid token, ulong fileSize)
    {
        const string sql = "INSERT INTO share(member_id, target, password, expire_time, comment, share_time, share_name, token, file_size) " +
                           "VALUES(@MemberID, @Target, @Password, @ExpireTime, @Comment, @ShareTime, @ShareName, UUID_TO_BIN(@Token), @FileSize)";
        using var conn = _connService.Connection;
        var res =await conn.ExecuteAsync(sql, new
        {
            MemberId = memberId,
            Target = target,
            Password = password,
            ExpireTime = expireTime, 
            Comment = comment,
            ShareTime = DateTime.UtcNow.Ticks,
            ShareName = shareName,
            Token = token.ToString(),
            FileSize = fileSize,
        });
        return res > 0;
    }

    public async Task<ShareResponseDto?> GetShareByToken(Guid token)
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
        return await conn.QueryFirstOrDefaultAsync<ShareResponseDto>(sql, new { Token = token.ToString() });
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

    public async Task<ShareDownloadDto?> GetShareDownloadDtoByToken(Guid token)
    {
        const string sql = "Select BIN_TO_UUID(m.directory) directory , s.target, s.expire_time expireTime, s.password " +
                           "FROM share AS s " +
                           "INNER JOIN member AS m " +
                           "ON s.member_id = m.member_id " +
                           "WHERE s.token = UUID_TO_BIN(@Token)";
        using var conn = _connService.Connection;
        return await conn.QueryFirstOrDefaultAsync<ShareDownloadDto?>(sql, new { Token = token });
    }

    public async Task<bool> TrySetShareExpireTimeToZero(ulong memberId, Guid token)
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

    public async Task<bool> TryUpdateShare(ulong memberId, Guid token, string? password, string? comment,
        ulong? expireTime, string? shareName)
    {
        const string sql = "UPDATE share " +
                           "SET password = @Password, expire_time = @Expire, comment = @Comment, share_name = @ShareName " +
                           "WHERE member_id = @Id AND token = UUID_TO_BIN(@Token)";
        using var conn = _connService.Connection;
        var res = await conn.ExecuteAsync(sql, new
        {
            Password = password,
            Expire = expireTime ?? (ulong)DateTime.MaxValue.Ticks,
            Comment = comment,
            ShareName = shareName,
            Token = token,
            Id = memberId,
        });
        return res > 0;  
    }

    public async Task<List<ShareResponseDto>> GetSharesByTargetFilePath(ulong memberid, string targetFilePath)
    {
        const string sql = "Select m.member_id ownerId, m.nickname ownerNick, " +
                           "s.share_time shareTime, s.expire_time expireTime, s.target target, " +
                           "s.share_name shareName, s.comment, BIN_TO_UUID(s.token) token, s.password, s.file_size filesize " +
                           "FROM share AS s " +
                           "INNER JOIN member AS m " +
                           "ON s.member_id = m.member_id " +
                           "WHERE s.member_id = @Id AND target = @Target ";
        using var conn = _connService.Connection;
        return (await conn.QueryAsync<ShareResponseDto>(sql, new { Id = memberid, Target = targetFilePath })).ToList();
    }
    
    public async Task<List<ShareResponseDto>> GetSharesInDirectory(ulong memberid, string targetDirectoryPath)
    {
        var targetPath = Path.Combine(targetDirectoryPath, "%");
        const string sql = "Select m.member_id ownerId, m.nickname ownerNick, " +
                           "s.share_time shareTime, s.expire_time expireTime, s.target target, " +
                           "s.share_name shareName, s.comment, BIN_TO_UUID(s.token) token, s.password, s.file_size filesize " +
                           "FROM share AS s " +
                           "INNER JOIN member AS m " +
                           "ON s.member_id = m.member_id " +
                           "WHERE s.member_id = @Id AND target Like @Target ";
        using var conn = _connService.Connection;
        return (await conn.QueryAsync<ShareResponseDto>(sql, new { Id = memberid, Target = targetPath })).ToList();
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
    
    public async Task<bool> TryDeleteShareInDirectory(ulong memberId, string targetDirectoryPath, int sharesCount)
    {
        var targetPath = Path.Combine(targetDirectoryPath, "%");
        const string sql = "DELETE FROM share " +
                           "WHERE member_id = @Id AND target Like @Target";
        using var conn = _connService.Connection;
        var res = await conn.ExecuteAsync(sql, new
        {
            Target = targetPath,
            Id = memberId,
        });
        return res == sharesCount;
    }

    public async Task<string?> GetPasswordHashByToken(Guid token)
    {
        var sql = "Select password FROM share WHERE token = UUID_TO_BIN(@Token)";
        using var conn = _connService.Connection;
        return await conn.QueryFirstOrDefaultAsync<string>(sql, new
        {
            Token = token,
        });
    }
    
    
    
}