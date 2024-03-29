using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.PasswordStore;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Dapper;
using MySql.Data.MySqlClient;

namespace cloudsharpback.Repository;

public class PasswordStoreValueRepository : IPasswordStoreValueRepository
{
    private readonly IDBConnectionFactory _connService;

    public PasswordStoreValueRepository(IDBConnectionFactory connService)
    {
        _connService = connService;
    }


    public async Task<PasswordStoreValueDto?> GetValueById(ulong id)
    {
        const string sql = @"
SELECT password_store_value_id Id,
       value_id ValueId,
       value_password ValuePassword,
       directory_id DirectoryId,
       encrypt_key_id KeyId,
       last_edited_time LastEditedTime,
       created_time CreatedTime
FROM password_store_value
WHERE password_store_value_id = @id;
";
        using var conn = _connService.MySqlConnection;
        return await conn.QueryFirstOrDefaultAsync<PasswordStoreValueDto?>(sql, new
        {
            id
        });
    }

    public async Task<List<PasswordStoreValueListItemDto>> GetValuesByDirectoryId(ulong dirId)
    {
        const string sql = @"
SELECT password_store_value_id Id,
       directory_id DirectoryId,
       encrypt_key_id KeyId,
       last_edited_time LastEditedTime,
       created_time CreatedTime
FROM password_store_value
WHERE directory_id = @dirId ;
";
        using var conn = _connService.MySqlConnection;
        var res = await conn.QueryAsync<PasswordStoreValueListItemDto>(sql, new { dirId });
        return res.ToList();
    }
    
    public async Task<List<PasswordStoreValueListItemDto>> GetValuesByKeyId(ulong keyId)
    {
        const string sql = @"
SELECT password_store_value_id Id,
       directory_id DirectoryId,
       encrypt_key_id KeyId,
       last_edited_time LastEditedTime,
       created_time CreatedTime
FROM password_store_value
WHERE encrypt_key_id = @keyId ;
";
        using var conn = _connService.MySqlConnection;
        var res = await conn.QueryAsync<PasswordStoreValueListItemDto>(sql, new { keyId });
        return res.ToList();
    }
    
    public async Task<List<PasswordStoreValueListItemDto>> GetValuesByKeyIdAndDirId(ulong dirId ,ulong keyId)
    {
        const string sql = @"
SELECT password_store_value_id Id,
       directory_id DirectoryId,
       encrypt_key_id KeyId,
       last_edited_time LastEditedTime,
       created_time CreatedTime
FROM password_store_value
WHERE encrypt_key_id = @keyId AND directory_id = @dirId;
";
        using var conn = _connService.MySqlConnection;
        var res = await conn.QueryAsync<PasswordStoreValueListItemDto>(sql, new { keyId, dirId });
        return res.ToList();
    }

    public async Task<bool> TryInsertValue(ulong dirId, ulong keyId, string? valueId, string valuePassword)
    {
        try
        {
            return await InsertValue(dirId, keyId, valueId, valuePassword);
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

    private async Task<bool> InsertValue(ulong dirId, ulong keyId, string? valueId, string valuePassword)
    {
        const string sql = @"
INSERT INTO password_store_value(directory_id, encrypt_key_id, value_id, value_password, last_edited_time, created_time) 
VALUES (@dirid, @keyid, @valueId, @valuePassword, @lastEditedTIme, @createdTime) ;
";
        using var conn = _connService.MySqlConnection;
        var res = await conn.ExecuteAsync(sql, new
        {
            dirId,
            keyId,
            valueId,
            valuePassword,
            lastEditedTime = DateTime.UtcNow.Ticks,
            createdTime = DateTime.UtcNow.Ticks,
        });
        return res > 0;   
    }

    public async Task<bool> UpdateValue(ulong itemId, string? valueId, string valuePassword)
    {
        const string sql = @"
UPDATE password_store_value
SET value_id = @valueId, value_password = @valuePassword, last_edited_time = @lastEditTIme
WHERE password_store_value_id = @itemId;
";
        using var conn = _connService.MySqlConnection;
        var res = await conn.ExecuteAsync(sql, new { itemId, valueId, valuePassword, lastEditTIme = DateTime.UtcNow.Ticks });
        return res > 0;
    }

    public async Task<bool> DeleteValue(ulong itemId)
    {
        const string sql = @"
DELETE FROM password_store_value
WHERE password_store_value_id = @itemId ;
";
        using var conn = _connService.MySqlConnection;
        var res = await conn.ExecuteAsync(sql, new
        {
            itemId
        });
        return res > 0;
    }
}