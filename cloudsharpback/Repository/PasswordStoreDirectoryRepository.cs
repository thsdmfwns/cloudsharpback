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
        const string SQL = @"
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
        return (await conn.QueryAsync<PasswordStoreDirDto>(SQL, new { memberId })).ToList();
    }

    public async Task<bool> InstertDir(ulong memberId, string name, string? comment, string? icon)
    {
        const string SQL = @"
INSERT INTO password_store_directory(name, comment, icon, last_edited_time, created_time, member_id)
VALUES (@name, @comment, @icon, @lastEdit, @created, @memberId);
";
        using var conn = _connService.Connection;
        var res = await conn.ExecuteAsync(SQL, new
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
}