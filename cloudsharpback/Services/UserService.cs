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
        private string ProfilePath;

        public UserService(IConfiguration configuration, IDBConnService connService, ILogger<IUserService> logger)
        {
            ProfilePath = configuration["File:ProfileImagePath"];
            if (!Directory.Exists(ProfilePath)) Directory.CreateDirectory(ProfilePath);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns>404 : fail to login</returns>
        /// <exception cref="HttpErrorException"></exception>
        public async Task<(HttpErrorDto? err, MemberDto? result)> Login(LoginDto loginDto)
        {
            try
            {
                var passwordHash = await GetPasswordHash(loginDto.Id);
                if (passwordHash is null
                    || !VerifyPassword(loginDto.Password, passwordHash))
                {
                    var res = new HttpErrorDto() { ErrorCode = 404 };
                    return (res, null);
                }
                var query = "SELECT member_id id, role_id role, email, nickname, BIN_TO_UUID(directory) directory, BIN_TO_UUID(profile_image_id) profileImageID " +
                    "FROM member " +
                    "WHERE id = @Id";
                using var conn = _connService.Connection;
                var result = await conn.QuerySingleAsync<MemberDto>(query, new { Id = loginDto.Id });
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
        /// <returns>415 : bad type, 409 : try again, 404: member not found</returns>
        public async Task<HttpErrorDto?> UploadProfileImage(IFormFile imageFile, MemberDto member)
        {
            try
            {
                var profileId = Guid.NewGuid();
                if (imageFile.ContentType.Split('/')[0] != "image")
                {
                    return new HttpErrorDto() { ErrorCode = 415, Message = "bad image type" };
                }
                var filepath = Path.Combine(ProfilePath, profileId.ToString());
                if (File.Exists(filepath))
                {
                    return new HttpErrorDto() { ErrorCode = 409, Message = "try again" };
                }
                using (var stream = System.IO.File.Create(filepath))
                {
                    await imageFile.CopyToAsync(stream);
                }

                using var conn = _connService.Connection;
                var sql = "UPDATE member " +
                    "SET profile_image_id = UUID_TO_BIN(@ProfileId), profile_image_type = @ProfileType " +
                    "WHERE member_id = @Id";
                var result = await conn.ExecuteAsync(sql, new
                {
                    ProfileId = profileId,
                    ProfileType = imageFile.ContentType,
                    Id = member.Id,
                });
                if (result <= 0)
                {
                    return new HttpErrorDto() { ErrorCode = 404, Message = "member not found" };
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to upload profile image",
                });
            }
        }
    }
}
