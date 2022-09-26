using cloudsharpback.Models;
using cloudsharpback.Utills;
using Dapper;

namespace cloudsharpback.Services
{
    public class UserService : IUserService
    {
        private readonly IDBConnService _connService;

        public UserService(IDBConnService connService)
        {
            _connService = connService;
        }

        public bool IdCheck(string id, out string? passwordHash)
        {
            var query = "SELECT password FROM member WHERE id = @Id";
            using var conn = _connService.Connection;
            passwordHash = conn.QuerySingleOrDefault<string?>(query, new { Id = id });
            return passwordHash != null;  
        }
        public bool IdCheck(string id)
        {
            var query = "SELECT password FROM member WHERE id = @Id";
            using var conn = _connService.Connection;
            return conn.Query(query, new { Id = id }).Any();
        }

        public bool TryLogin(LoginDto loginDto, out MemberDto? member)
        {
            if (!IdCheck(loginDto.Id, out var passwordHash)
                || !PasswordEncrypt.VerifyPassword(loginDto.Password, passwordHash))
            {
                member = null;
                return false;
            }
            var query = "SELECT member_id, role_id, email, nickname " +
                "FROM member " +
                "WHERE id = @Id";
            using var conn = _connService.Connection;
            member = conn.QuerySingleOrDefault<MemberDto?>(query, new { Id = loginDto.Id });
            return member != null;
        }

        public bool TryRegister(RegisterDto registerDto)
        {
            if (IdCheck(registerDto.Login.Id)) return false;
            using var conn = _connService.Connection;
            var query = "INSERT INTO member(id, password, nickname, role_id, email)" +
                "VALUES(@Id, @Pw, @Nick, @Role, @Email)";
            return conn.Execute(query, new
            {
                Id = registerDto.Login.Id,
                Pw = PasswordEncrypt.EncryptPassword(registerDto.Login.Password),
                Nick = registerDto.Nickname,
                Role = registerDto.Role,
                Email = registerDto.Email,
            }) != 0;
        }
    }
}
