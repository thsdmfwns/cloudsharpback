using cloudsharpback.Models;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using Dapper;

namespace cloudsharpback.Repository;

public class MemberRepository : IMemberRepository
{
    private readonly IDBConnService _connService;

    public MemberRepository(IDBConnService connService)
    {
        _connService = connService;
    }

    public async Task<MemberDto?> GetMemberById(ulong id)
    {
        var query = "SELECT member_id id, role_id role, email, nickname, " +
                    "BIN_TO_UUID(directory) directory, profile_image profileImage " +
                    "FROM member " +
                    "WHERE member_id = @Id";
        using var conn = _connService.Connection;
        return await conn.QuerySingleOrDefaultAsync<MemberDto>(query, new { Id = id });
    }

    public async Task<bool> TryUpdateMemberProfileImage(ulong id, string imageFileName)
    {
        var sql = "UPDATE member " +
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
        var sql = "UPDATE member " +
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

    public async Task<string?> GetMemberPasswordHashById(ulong id)
    {
        var sql = "SELECT password FROM member WHERE member_id = @Id";
        using var conn = _connService.Connection;
        return await conn.QuerySingleOrDefaultAsync<string?>(sql, new { Id = id });
    }

    public async Task<bool> TryUpdateMemberPassword(ulong id, string password)
    {
        var sql = "UPDATE member SET password = @Password WHERE member_id = @Id";
        using var conn = _connService.Connection;
        var result = await conn.ExecuteAsync(sql, new { Password = password, Id = id});
        return result > 0;
    }

    public async Task<bool> TryAddMember(RegisterDto registerDto, uint role)
    {
        using var conn = _connService.Connection;
        var query = "INSERT INTO member(id, password, nickname, role_id, email, directory) " +
                    "VALUES(@Id, @Pw, @Nick, @Role, @Email, UUID_TO_BIN(@Directory))";
        var result = await conn.ExecuteAsync(query, new
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