using cloudsharpback.Models;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using Dapper;

namespace cloudsharpback.Repository;

public class PasswordStoreKeyRepository : IPasswordStoreKeyRepository
{
    private readonly IDBConnService _connService;

    public PasswordStoreKeyRepository(IDBConnService connService)
    {
        _connService = connService;
    }

    public async Task<PasswordStoreKeyDto?> GetKeyById(ulong memberId, ulong keyId)
    {
        const string sql = @"
SELECT  password_store_key_id Id, 
        owner_id OwnerId, 
        public_key PublicKey, 
        private_key privateKey, 
        encrypt_algorithm EncryptAlgorithmValue,
        name AS Name,
        comment AS Comment,
        created_time CreatedTime
FROM password_store_keys
WHERE password_store_key_id = @keyId AND owner_id = @memberId;
";
        using var conn = _connService.Connection;
        return await conn.QueryFirstOrDefaultAsync<PasswordStoreKeyDto?>(sql, new { keyId, memberId });
    }

    public async Task<List<PasswordStoreKeyListItemDto>> GetKeyListByMemberId(ulong memberId)
    {
        const string sql = @"
SELECT  password_store_key_id Id, 
        owner_id OwnerId, 
        encrypt_algorithm EncryptAlgorithmValue,
        name AS Name,
        comment AS Comment,
        created_time CreatedTime
FROM password_store_keys
WHERE owner_id  = @memberId;
";
        using var conn = _connService.Connection;
        var res = await conn.QueryAsync<PasswordStoreKeyListItemDto>(sql, new { memberId });
        return res.ToList();
    }

    public async Task<bool> InsertKey(ulong memberId, int encryptAlgorithm, string? publicKey, string privateKey,
        string name, string? comment)
    {
        const string sql = @"
Insert Into password_store_keys(owner_id, encrypt_algorithm, public_key, private_key, name, comment, created_time)
VALUES (@memberId, @encryptAlgorithm, @publicKey, @privateKey, @name, @comment, @created);
";
        using var conn = _connService.Connection;
        var res = await conn.ExecuteAsync(sql, new
        {
            memberId,
            encryptAlgorithm,
            privateKey,
            publicKey,
            name,
            comment,
            created = DateTime.UtcNow.Ticks
        });
        return res > 0;
    }

    public async Task<bool> DeleteKeyById(ulong memberId, ulong itemId)
    {
        const string sql = @"
DELETE FROM password_store_keys
WHERE owner_id = @memberId AND password_store_key_id = @itemId ;
";
        using var conn = _connService.Connection;
        var res = await conn.ExecuteAsync(sql, new { memberId, itemId });
        return res > 0;
    }
}