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


    public async Task<List<PasswordStoreValueDto>> GetPasswordStoreValuesByDirectoryId(ulong dirId)
    {
        const string sql = @"
SELECT password_store_value_id Id,
       value_id ValueId,
       value_password ValuePassword,
       directory_id DirecotryId,
       encrypt_key_id KeyId
FROM password_store_value
WHERE directory_id = @dirId ;
";
        using var conn = _connService.Connection;
        var res = await conn.QueryAsync<PasswordStoreValueDto>(sql, new { dirId });
        return res.ToList();
    }
    
    public async Task<List<PasswordStoreValueDto>> GetPasswordStoreValuesByKeyId(ulong keyId)
    {
        const string sql = @"
SELECT password_store_value_id Id,
       value_id ValueId,
       value_password ValuePassword,
       directory_id DirecotryId,
       encrypt_key_id KeyId
FROM password_store_value
WHERE encrypt_key_id = @keyId ;
";
        using var conn = _connService.Connection;
        var res = await conn.QueryAsync<PasswordStoreValueDto>(sql, new { keyId });
        return res.ToList();
    }
    
    public async Task<List<PasswordStoreValueDto>> GetPasswordStoreValuesByKeyIdAndDirId(ulong dirId ,ulong keyId)
    {
        const string sql = @"
SELECT password_store_value_id Id,
       value_id ValueId,
       value_password ValuePassword,
       directory_id DirecotryId,
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
}