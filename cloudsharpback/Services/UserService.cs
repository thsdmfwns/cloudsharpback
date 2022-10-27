using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
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

        async Task<string?> GetPasswordHash(string id)
        {
            var query = "SELECT password FROM member WHERE id = @Id";
            using var conn = _connService.Connection;
            return await conn.QuerySingleOrDefaultAsync<string?>(query, new { Id = id });
        }
        public async Task<bool> IdCheck(string id)
        {
            var query = "SELECT password FROM member WHERE id = @Id";
            using var conn = _connService.Connection;
            return (await conn.QueryAsync(query, new { Id = id })).Any();
        }

        public async Task<(ServiceResult response, MemberDto? result)> Login(LoginDto loginDto)
        {
            try
            {
                var passwordHash = await GetPasswordHash(loginDto.Id);
                if (passwordHash is null
                    || !VerifyPassword(loginDto.Password, passwordHash))
                {
                    var res = new ServiceResult() { ErrorCode = 404 };
                    return (res, null);
                }
                var query = "SELECT member_id id, role_id role, email, nickname, BIN_TO_UUID(directory) directory " +
                    "FROM member " +
                    "WHERE id = @Id";
                using var conn = _connService.Connection;
                var result = await conn.QuerySingleAsync<MemberDto>(query, new { Id = loginDto.Id });
                return (ServiceResult.Sucess, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new ServiceResult
                {
                    ErrorCode = 500,
                    Message = "fail to login",
                });
            }

        }

        public async Task<(ServiceResult response, string? directoryId)> Register(RegisterDto registerDto, ulong role)
        {
            try
            {
                registerDto.Pw = EncryptPassword(registerDto.Pw);
                var directoryId = Guid.NewGuid().ToString();
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
                    Directory = directoryId,
                }) != 0;
                if (!result)
                {
                    var res = new ServiceResult() { ErrorCode = 400 };
                    return (res, null);
                }
                return (ServiceResult.Sucess, directoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new ServiceResult
                {
                    ErrorCode = 500,
                    Message = "fail to register",
                });
            }
        }
    }
}
