using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
using Dapper;
using System.Net.Mail;

namespace cloudsharpback.Services
{
    public class MemberService : IMemberService
    {
        private readonly IDBConnService _connService;
        private readonly ILogger _logger;
        private readonly string _profilePath;

        public MemberService(IDBConnService connService, ILogger<IMemberService> logger, IPathStore pathStore)
        {
            _profilePath = pathStore.ProfilePath;
            if (!Directory.Exists(_profilePath)) Directory.CreateDirectory(_profilePath);
            _connService = connService;
            _logger = logger;
        }

        /// <returns>404 : fail to login</returns>
        public async Task<(HttpErrorDto? err, MemberDto? result)> GetMemberById(ulong id)
        {
            try
            {
                var query = "SELECT member_id id, role_id role, email, nickname, " +
                    "BIN_TO_UUID(directory) directory, profile_image profileImage " +
                    "FROM member " +
                    "WHERE member_id = @Id";
                using var conn = _connService.Connection;
                var result = await conn.QuerySingleOrDefaultAsync<MemberDto>(query, new { Id = id });
                if (result is null)
                {
                    var err = new HttpErrorDto() { ErrorCode = 404, Message = "member not found" };
                    return (err, null);
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
                    Message = "fail to get member",
                });
            }
        }

        /// <returns>415 : bad type, 409 : try again, 404: member not found</returns>
        public async Task<HttpErrorDto?> UploadProfileImage(IFormFile imageFile, MemberDto member)
        {
            try
            {
                var profileId = Guid.NewGuid();
                var extension = Path.GetExtension(imageFile.FileName);
                var mime = MimeTypeUtil.GetMimeType(extension);
                if (mime is null 
                    || mime.Split('/')[0] != "image")
                {
                    return new HttpErrorDto() { ErrorCode = 415, Message = "bad type" };
                }
                var filename = profileId.ToString() + extension;
                var filepath = Path.Combine(_profilePath, filename);
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
                    "SET profile_image = @Filename " +
                    "WHERE member_id = @Id";
                var result = await conn.ExecuteAsync(sql, new
                {
                    Filename = filename,
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

        public HttpErrorDto? DownloadProfileImage(string profileImage, out FileStream? fileStream, out string? contentType)
        {
            try
            {
                fileStream = null;
                var filepath = Path.Combine(_profilePath, profileImage);
                contentType = MimeTypeUtil.GetMimeType(profileImage);
                if (!File.Exists(filepath)
                    || contentType is null)
                {
                    return new HttpErrorDto() { ErrorCode = 404, Message = "file not found" };
                }
                fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to download file",
                });
            }
        }

        public async Task<HttpErrorDto?> UpadteNickname(MemberDto member, string changeNick)
        {
            try
            {
                if (member.Email.Equals(changeNick))
                {
                    return null;
                }

                var sql = "UPDATE member " +
                "SET nickname = @ChangeNick " +
                "WHERE member_id = @Id";
                using var conn = _connService.Connection;
                var result = await conn.ExecuteAsync(sql, new
                {
                    ChangeNick = changeNick,
                    Id = member.Id
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
                    Message = "fail to update nick",
                });
            }
        }

        public async Task<HttpErrorDto?> UpadteEmail(MemberDto member, string changeEmail)
        {
            try
            {
                // validate email
                try
                {
                    var temp = new MailAddress(changeEmail);
                }
                catch (Exception)
                {
                    return new HttpErrorDto() { ErrorCode = 400, Message = "bad email" };
                }

                var sql = "UPDATE member " +
                "SET email = @ChangeEmail " +
                "WHERE member_id = @Id";
                using var conn = _connService.Connection;
                var result = await conn.ExecuteAsync(sql, new
                {
                    ChangeEmail = changeEmail,
                    Id = member.Id
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
                    Message = "fail to update email",
                });
            }
        }

        public async Task<(HttpErrorDto? err, bool result)> CheckPassword(MemberDto member, string password)
        {
            try
            {
                var sql = "SELECT password FROM member WHERE member_id = @Id";
                using var conn = _connService.Connection;
                var passwordHash = await conn.QuerySingleOrDefaultAsync<string?>(sql, new { Id = member.Id });
                if (passwordHash is null)
                {
                    var err = new HttpErrorDto() { ErrorCode = 404, Message = "member not found" };
                    return (err, false);
                }
                return (null, PasswordEncrypt.VerifyPassword(password, passwordHash));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to check password",
                });
                throw;
            }

        }

        public async Task<HttpErrorDto?> UpdatePassword(MemberDto member, UpadtePasswordDto requset)
        {
            try
            {
                var checkresult = await CheckPassword(member, requset.Original);
                if (checkresult.err is not null)
                {
                    return checkresult.err;
                }
                if (!checkresult.result)
                {
                    return new HttpErrorDto() { ErrorCode = 400, Message = "check password" };
                }

                var password = PasswordEncrypt.EncryptPassword(requset.ChangeTo);
                var sql = "UPDATE member SET password = @Password WHERE member_id = @Id";
                using var conn = _connService.Connection;
                var result = await conn.ExecuteAsync(sql, new { Password = password, Id = member.Id });
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
                    Message = "fail to update password",
                });
                throw;
            }
        }
    }
}
