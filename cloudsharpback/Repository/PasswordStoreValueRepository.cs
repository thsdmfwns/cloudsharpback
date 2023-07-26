using cloudsharpback.Models;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services;
using cloudsharpback.Services.Interfaces;
using Dapper;

namespace cloudsharpback.Repository;

public class PasswordStoreValueRepository : IPasswordStoreValueRepository
{
    private readonly IDBConnService _connService;

    public PasswordStoreValueRepository(IDBConnService connService)
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
       encrypt_key_id KeyId
FROM password_store_value
WHERE password_store_value_id = @id;
";
        using var conn = _connService.Connection;
        return await conn.QueryFirstOrDefaultAsync<PasswordStoreValueDto?>(sql, new
        {
            id
        });
    }

    public async Task<List<PasswordStoreValueDto>> GetValuesByDirectoryId(ulong dirId)
    {
        const string sql = @"
SELECT password_store_value_id Id,
       value_id ValueId,
       value_password ValuePassword,
       directory_id DirectoryId,
       encrypt_key_id KeyId
FROM password_store_value
WHERE directory_id = @dirId ;
";
        using var conn = _connService.Connection;
        var res = await conn.QueryAsync<PasswordStoreValueDto>(sql, new { dirId });
        return res.ToList();
    }
    
    public async Task<List<PasswordStoreValueDto>> GetValuesByKeyId(ulong keyId)
    {
        const string sql = @"
SELECT password_store_value_id Id,
       value_id ValueId,
       value_password ValuePassword,
       directory_id DirectoryId,
       encrypt_key_id KeyId
FROM password_store_value
WHERE encrypt_key_id = @keyId ;
";
        using var conn = _connService.Connection;
        var res = await conn.QueryAsync<PasswordStoreValueDto>(sql, new { keyId });
        return res.ToList();
    }
    
    public async Task<List<PasswordStoreValueDto>> GetValuesByKeyIdAndDirId(ulong dirId ,ulong keyId)
    {
        const string sql = @"
SELECT password_store_value_id Id,
       value_id ValueId,
       value_password ValuePassword,
       directory_id DirectoryId,
       encrypt_key_id KeyId
FROM password_store_value
WHERE encrypt_key_id = @keyId AND directory_id = @dirId;
";
        using var conn = _connService.Connection;
        var res = await conn.QueryAsync<PasswordStoreValueDto>(sql, new { keyId, dirId });
        return res.ToList();
    }

    public async Task<bool> InsertValue(ulong dirId, ulong keyId, string? valueId, string valuePassword)
    {
        const string sql = @"
INSERT INTO password_store_value(directory_id, encrypt_key_id, value_id, value_password) 
VALUES (@dirid, @keyid, @valueId, @valuePassword) ;
";
        using var conn = _connService.Connection;
        var res = await conn.ExecuteAsync(sql, new
        {
            dirId,
            keyId,
            valueId,
            valuePassword
        });
        return res > 0;
    }

    public async Task<bool> DeleteValue(ulong itemId)
    {
        const string sql = @"
DELETE FROM password_store_value
WHERE password_store_value_id = @itemId ;
";
        using var conn = _connService.Connection;
        var res = await conn.ExecuteAsync(sql, new
        {
            itemId
        });
        return res > 0;
    }
}