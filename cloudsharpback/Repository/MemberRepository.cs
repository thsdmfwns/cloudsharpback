using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using Dapper;
using MySql.Data.MySqlClient;

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
        using var conn = _connService.MySqlConnection;
        return await conn.QuerySingleOrDefaultAsync<MemberDto>(sql, new { Id = id });
    }
    
    public async Task<MemberDto?> GetMemberByLoginId(string id)
    {
        const string sql = "SELECT member_id id, role_id role, email, nickname, " +
                    "BIN_TO_UUID(directory) directory, profile_image profileImage " +
                    "FROM member " +
                    "WHERE id = @Id";
        using var conn = _connService.MySqlConnection;
        return await conn.QuerySingleOrDefaultAsync<MemberDto>(sql, new { Id = id });
    }

    public async Task<bool> TryUpdateMemberProfileImage(ulong id, string imageFileName)
    {
        const string sql = "UPDATE member " +
                           "SET profile_image = @Filename " +
                           "WHERE member_id = @Id";
        using var conn = _connService.MySqlConnection;
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
        using var conn = _connService.MySqlConnection;
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
        using var conn = _connService.MySqlConnection;
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
        using var conn = _connService.MySqlConnection;
        return await conn.QuerySingleOrDefaultAsync<string?>(sql, new { Id = id });
    }
    
    public async Task<string?> GetMemberPasswordHashByLoginId(string id)
    {
        const string sql = "SELECT password FROM member WHERE id = @Id";
        using var conn = _connService.MySqlConnection;
        return await conn.QuerySingleOrDefaultAsync<string?>(sql, new { Id = id });
    }
    
    public async Task<bool> TryLoginIdDuplicate(string id)
    {
        const string sql = "SELECT member_id FROM member WHERE id = @id";
        using var conn = _connService.MySqlConnection;
        return (await conn.QueryAsync(sql, new { Id = id })).Any();
    }
    
    public async Task<bool> TryUpdateMemberPassword(ulong id, string password)
    {
        const string sql = "UPDATE member SET password = @Password WHERE member_id = @Id";
        using var conn = _connService.MySqlConnection;
        var result = await conn.ExecuteAsync(sql, new { Password = password, Id = id});
        return result > 0;
    }

    public async Task<bool> TryAddMember(RegisterDto dto, ulong role)
        => await TryAddMember(dto.Id, dto.Pw, dto.Nick, dto.Email, Guid.NewGuid(), role, null);
    
    public async Task<bool> TryAddMember(string id, string pw, string nick, string email, Guid dir, ulong role,
        string? profileImage)
    {
        try
        {
            return await AddMember(id, pw, nick, email, dir, role, profileImage);
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

    private async Task<bool> AddMember(string id, string pw, string nick, string email, Guid dir, ulong role,
        string? profileImage)
    {
        using var conn = _connService.MySqlConnection;
        const string sql = "INSERT INTO member(id, password, nickname, role_id, email, directory, profile_image) " +
                    "VALUES(@Id, @Pw, @Nick, @Role, @Email, UUID_TO_BIN(@Directory), @ProfileImage)";
        var result = await conn.ExecuteAsync(sql, new
        {
            Id = id,
            Pw = pw,
            Nick = nick,
            Role = role,
            Email = email,
            Directory = dir.ToString(),
            ProfileImage = profileImage
        });
        return result > 0;
    }
}