using cloudsharpback.Models;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using Dapper;

namespace cloudsharpback.Repository;

public class PasswordStoreDirectoryRepository : IPasswordStoreDirectoryRepository
{
    private readonly IDBConnService _connService;

    public PasswordStoreDirectoryRepository(IDBConnService connService)
    {
        _connService = connService;
    }

    public async Task<List<PasswordStoreDirDto>> GetDirListByMemberId(ulong memberId)
    {
        const string sql = @"
SELECT password_directory_id Id, 
       name as Name, 
       comment as Comment, 
       icon as Icon, 
       last_edited_time LastEditTIme, 
       created_time CreatedTIme, 
       member_id OwnerId
FROM password_store_directory 
WHERE member_id = @memberId ;
";
        using var conn = _connService.Connection;
        return (await conn.QueryAsync<PasswordStoreDirDto>(sql, new { memberId })).ToList();
    }

    public async Task<PasswordStoreDirDto?> GetDirById(ulong memberId, ulong dirId)
    {
        const string sql = @"
SELECT password_directory_id Id, 
       name as Name, 
       comment as Comment, 
       icon as Icon, 
       last_edited_time LastEditTIme, 
       created_time CreatedTIme, 
       member_id OwnerId
FROM password_store_directory 
WHERE password_directory_id = @dirId AND member_id = @memberId;
";
        using var conn = _connService.Connection;
        return await conn.QueryFirstOrDefaultAsync<PasswordStoreDirDto?>(sql, new { dirId, memberId });
    }

    public async Task<bool> InsertDir(ulong memberId, string name, string? comment, string? icon)
    {
        const string sql = @"
INSERT INTO password_store_directory(name, comment, icon, last_edited_time, created_time, member_id)
VALUES (@name, @comment, @icon, @lastEdit, @created, @memberId);
";
        using var conn = _connService.Connection;
        var res = await conn.ExecuteAsync(sql, new
        {
            name,
            comment,
            icon,
            lastEdit = DateTime.UtcNow.Ticks,
            created = DateTime.UtcNow.Ticks,
            memberId
        });
        return res > 0;
    }

    public async Task<bool> InsertDir(ulong memberId, PasswordStoreDirInsertDto dto)
        => await InsertDir(memberId, dto.Name, dto.Comment, dto.Icon);

    public async Task<bool> DeleteDir(ulong memberId, ulong id)
    {
        const string sql = @"
DELETE FROM password_store_directory
WHERE password_directory_id = @id AND member_id = @memberId ;
";
        using var conn = _connService.Connection;
        var res = await conn.ExecuteAsync(sql, new { id, memberId });
        return res > 0;
    }

    public async Task<bool> UpdateDir(ulong memberId, ulong itemId, string name, string? comment, string? icon)
    {
        const string sql = @"
UPDATE password_store_directory 
SET name = @name, comment = @comment, icon = @icon, last_edited_time = @last
WHERE password_directory_id = @itemId AND member_id = @memberId ;
";
        using var conn = _connService.Connection;
        var res = await conn.ExecuteAsync(sql,
            new { name, comment, icon, itemId, memberId, last = DateTime.UtcNow.Ticks });
        
        return res > 0;
    }
}