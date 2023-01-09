using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
using Dapper;

namespace cloudsharpback.Services
{
    public class UserService
    {
        private readonly IDBConnService _connService;
        private readonly ILogger _logger;

        public UserService(IDBConnService connService, ILogger logger)
        {
            _connService = connService;
            _logger = logger;
        }


        public async Task<(HttpErrorDto? err, MemberDto? result)> Login(LoginDto loginDto)
        {
            try
            {
                var passwordHash = PasswordEncrypt.EncryptPassword(loginDto.Id);
                if (passwordHash is null
                    || !PasswordEncrypt.VerifyPassword(loginDto.Password, passwordHash))
                {
                    var res = new HttpErrorDto() { ErrorCode = 404, Message = "login fail" };
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
    }
}
