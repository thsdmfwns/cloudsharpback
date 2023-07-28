using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using Dapper;

namespace cloudsharpback.Repository;

public class MemberRepository : IMemberRepository
{
    private readonly IDBConnectionFactory _connService;

    public MemberRepository(IDBConnectionFactory connService)
    {
        _connService = connService;
    }

    public async Task<MemberDto?> GetMemberById(ulong id)
    {
        const string sql = "SELECT member_id id, role_id role, email, nickname, " +
                    "BIN_TO_UUID(directory) directory, profile_image profileImage " +
                    "FROM member " +
                    "WHERE member_id = @Id";
        using var conn = _connService.Connection;
        return await conn.QuerySingleOrDefaultAsync<MemberDto>(sql, new { Id = id });
    }
    
    public async Task<MemberDto?> GetMemberByLoginId(string id)
    {
        const string sql = "SELECT member_id id, role_id role, email, nickname, " +
                    "BIN_TO_UUID(directory) directory, profile_image profileImage " +
                    "FROM member " +
                    "WHERE id = @Id";
        using var conn = _connService.Connection;
        return await conn.QuerySingleOrDefaultAsync<MemberDto>(sql, new { Id = id });
    }

    public async Task<bool> TryUpdateMemberProfileImage(ulong id, string imageFileName)
    {
        const string sql = "UPDATE member " +
                           "SET profile_image = @Filename " +
                           "WHERE member_id = @Id";
        using var conn = _connService.Connection;
        var result = await conn.ExecuteAsync(sql, new
        {
            Filename = imageFileName,
            Id = id,
        });
        return result > 0;
    }

    public async Task<bool> TryUpdateMemberNickname(ulong id, string nickname)
    {
        const string sql = "UPDATE member " +
                           "SET nickname = @ChangeNick " +
                           "WHERE member_id = @Id";
        using var conn = _connService.Connection;
        var result = await conn.ExecuteAsync(sql, new
        {
            ChangeNick = nickname,
            Id = id
        });
        return result > 0;
    }
    
    public async Task<bool> TryUpdateMemberEmail(ulong id, string email)
    {
        const string sql = "UPDATE member " +
                     "SET email = @Email " +
                     "WHERE member_id = @Id";
        using var conn = _connService.Connection;
        var result = await conn.ExecuteAsync(sql, new
        {
            Email = email,
            Id = id
        });
        return result > 0;
    }

    public async Task<string?> GetMemberPasswordHashById(ulong id)
    {
        const string sql = "SELECT password FROM member WHERE member_id = @Id";
        using var conn = _connService.Connection;
        return await conn.QuerySingleOrDefaultAsync<string?>(sql, new { Id = id });
    }
    
    public async Task<string?> GetMemberPasswordHashByLoginId(string id)
    {
        const string sql = "SELECT password FROM member WHERE id = @Id";
        using var conn = _connService.Connection;
        return await conn.QuerySingleOrDefaultAsync<string?>(sql, new { Id = id });
    }
    
    public async Task<bool> TryLoginIdDuplicate(string id)
    {
        const string sql = "SELECT member_id FROM member WHERE id = @id";
        using var conn = _connService.Connection;
        return (await conn.QueryAsync(sql, new { Id = id })).Any();
    }
    
    public async Task<bool> TryUpdateMemberPassword(ulong id, string password)
    {
        const string sql = "UPDATE member SET password = @Password WHERE member_id = @Id";
        using var conn = _connService.Connection;
        var result = await conn.ExecuteAsync(sql, new { Password = password, Id = id});
        return result > 0;
    }

    public async Task<bool> TryAddMember(RegisterDto registerDto, ulong role)
    {
        using var conn = _connService.Connection;
        const string sql = "INSERT INTO member(id, password, nickname, role_id, email, directory) " +
                    "VALUES(@Id, @Pw, @Nick, @Role, @Email, UUID_TO_BIN(@Directory))";
        var result = await conn.ExecuteAsync(sql, new
        {
            Id = registerDto.Id,
            Pw = registerDto.Pw,
            Nick = registerDto.Nick,
            Role = role,
            Email = registerDto.Email,
            Directory = Guid.NewGuid().ToString(),
        });
        return result > 0;
    }
}