using cloudsharpback.Models;
using cloudsharpback.Utills;
using Dapper;
using System.Diagnostics.Metrics;

namespace cloudsharpback.Services
{
    public class UserService : IUserService
    {
        private readonly IDBConnService _connService;
        private readonly ILogger _logger;

        public UserService(IDBConnService connService, ILogger<IUserService> logger)
        {
            _connService = connService;
            _logger = logger;
        }

        string EncryptPassword(string password) => Base64.Encode(Encrypt.EncryptByBCrypt(password));
        bool VerifyPassword(string password, string hash) => Encrypt.VerifyBCrypt(password, Base64.Decode(hash));

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
            try
            {
                if (!IdCheck(loginDto.Id, out var passwordHash)
                    || passwordHash is null
                    || !VerifyPassword(loginDto.Password, passwordHash))
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
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                member = null;
                return false;
            }

        }

        public bool TryRegister(RegisterDto registerDto, ulong role)
        {
            try
            {
                registerDto.Pw = EncryptPassword(registerDto.Pw);
                using var conn = _connService.Connection;
                var query = "INSERT INTO member(id, password, nickname, role_id, email)" +
                    "VALUES(@Id, @Pw, @Nick, @Role, @Email)";
                return conn.Execute(query, new
                {
                    Id = registerDto.Id,
                    Pw = registerDto.Pw,
                    Nick = registerDto.Nick,
                    Role = role,
                    Email = registerDto.Email,
                }) != 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                return false;
            }
        }
    }
}
