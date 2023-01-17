using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
using Dapper;

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

        public async Task<(HttpErrorDto? err, MemberDto? result)> Login(LoginDto loginDto)
        {
            try
            {
                var passwordHash = await GetPasswordHash(loginDto.Id);
                if (passwordHash is null
                    || !PasswordEncrypt.VerifyPassword(loginDto.Password, passwordHash))
                {
                    var res = new HttpErrorDto() { ErrorCode = 401, Message = "login fail" };
                    return (res, null);
                }
                var query = "SELECT member_id id, role_id role, email, nickname, " +
                    "BIN_TO_UUID(directory) directory, profile_image profileImage " +
                    "FROM member " +
                    "WHERE id = @Id";
                using var conn = _connService.Connection;
                var result = await conn.QuerySingleOrDefaultAsync<MemberDto>(query, new { Id = loginDto.Id });
                if (result is null)
                {
                    var err = new HttpErrorDto() { ErrorCode = 404, Message = "member not found" };
                }
                return (null, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to login",
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="registerDto"></param>
        /// <param name="role"></param>
        /// <returns>404 : bad json </returns>
        /// <exception cref="HttpErrorException"></exception>
        public async Task<(HttpErrorDto? err, string? directoryId)> Register(RegisterDto registerDto, ulong role)
        {
            try
            {
                registerDto.Pw = PasswordEncrypt.EncryptPassword(registerDto.Pw);
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
                    var res = new HttpErrorDto() { ErrorCode = 400 };
                    return (res, null);
                }
                return (null, directoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to register",
                });
            }
        }

    }
}
