using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
using Dapper;
using MySqlX.XDevAPI.Relational;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Ocsp;
using System.Diagnostics.Metrics;
using System.IO;
using Ubiety.Dns.Core;

namespace cloudsharpback.Services
{
    public class ShareService : IShareService
    {
        private readonly IDBConnService _connService;
        private readonly ILogger _logger;
        private string DirectoryPath;

        public ShareService(IConfiguration configuration, IDBConnService connService, ILogger<IShareService> logger)
        {
            DirectoryPath = configuration["File:DirectoryPath"];
            _connService = connService;
            _logger = logger;
        }

        string EncryptPassword(string password) => Base64.Encode(Encrypt.EncryptByBCrypt(password));
        bool VerifyPassword(string password, string hash) => Encrypt.VerifyBCrypt(password, Base64.Decode(hash));
        string userPath(string directoryId) => Path.Combine(DirectoryPath, directoryId);
        bool FileExist(string filePath) => System.IO.File.Exists(filePath);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="req"></param>
        /// <returns>404 : no file for share</returns>
        /// <exception cref="HttpErrorException"></exception>
        public async Task<HttpErrorDto?> Share(MemberDto member, ShareRequestDto req)
        {
            try
            {
                var filepath = Path.Combine(userPath(member.Directory), req.Target);
                if (!FileExist(filepath))
                {
                    return new HttpErrorDto
                    {
                        ErrorCode = 404,
                        Message = $"no file for share",
                    };
                }
                var fileinfo = new FileInfo(filepath);
                var password = req.Password;
                if (password is not null)
                {
                    password = EncryptPassword(password);
                }
                var sql = "INSERT INTO share(member_id, target, password, expire_time, comment, share_time, share_name, token, file_size) " +
                    "VALUES(@MemberID, @Target, @Password, @ExpireTime, @Comment, @ShareTime, @ShareName, UUID_TO_BIN(@Token), @FileSize)";
                using var conn = _connService.Connection;
                var token = Guid.NewGuid().ToString();
                await conn.ExecuteAsync(sql, new
                {
                    MemberId = member.Id,
                    Target = req.Target,
                    Password = password,
                    ExpireTime = req.ExpireTime ?? (ulong)DateTime.MaxValue.Ticks,
                    Comment = req.Comment,
                    ShareTime = DateTime.UtcNow.Ticks,
                    ShareName = req.ShareName,
                    Token = token,
                    FileSize = (ulong)fileinfo.Length,
                });
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to sharing",
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns>410 : expired share </returns>
        /// <exception cref="HttpErrorException"></exception>
        public async Task<(HttpErrorDto? err, ShareResponseDto? result)> GetShareAsync(string token)
        {
            try
            {
                //ulong id, ulong ownerId, string ownerNick, ulong shareTime, ulong? expireTime, string target, string? shareName, string? comment
                var sql = "Select m.member_id ownerId, m.nickname ownerNick, " +
                    "s.share_time shareTime, s.expire_time expireTime, s.target target, " +
                    "s.share_name shareName, s.comment, BIN_TO_UUID(s.token) token, s.password, s.file_size filesize " +
                    "FROM share AS s " +
                    "INNER JOIN member AS m " +
                    "ON s.member_id = m.member_id " +
                    "WHERE s.token = UUID_TO_BIN(@Token)";
                using var conn = _connService.Connection;
                var result = await conn.QueryFirstOrDefaultAsync<ShareResponseDto>(sql, new { Token = token });
                if (result.ExpireTime < (ulong)DateTime.UtcNow.Ticks)
                {
                    var err = new HttpErrorDto()
                    {
                        ErrorCode = 410,
                        Message = "expired share",
                    };
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
                    Message = "fail to get shares",
                });
            }
        }

        public async Task<List<ShareResponseDto>> GetSharesAsync(MemberDto member)
        {
            try
            {
                var sql = "Select m.member_id ownerId, m.nickname ownerNick, " +
                    "s.share_time shareTime, s.expire_time expireTime, s.target target, " +
                    "s.share_name shareName, s.comment, BIN_TO_UUID(s.token) token, s.password, s.file_size filesize " +
                    "FROM share AS s " +
                    "INNER JOIN member AS m " +
                    "ON s.member_id = m.member_id " +
                    "WHERE s.member_id = @ID AND s.expire_time >= @Now";
                using var conn = _connService.Connection;
                var result = await conn.QueryAsync<ShareResponseDto>(sql, new { ID = member.Id, Now = (ulong)DateTime.UtcNow.Ticks });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to get share",
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="password"></param>
        /// <returns>404 : file doesnt exist , 403 : bad password, 410 : expired share</returns>
        /// <exception cref="HttpErrorException"></exception>
        public async Task<(HttpErrorDto? err, FileStream? result)> DownloadShareAsync(string token, string? password)
        {
            try
            {
                var sql = "Select BIN_TO_UUID(m.directory) directory , s.target, s.expire_time expireTime, s.password " +
                    "FROM share AS s " +
                    "INNER JOIN member AS m " +
                    "ON s.member_id = m.member_id " +
                    "WHERE s.token = UUID_TO_BIN(@Token)";
                using var conn = _connService.Connection;
                var dto = await conn.QueryFirstOrDefaultAsync<ShareDownloadDto>(sql, new { Token = token });
                var filepath = Path.Combine(userPath(dto.Directory), dto.Target);
                if (dto is null || !FileExist(filepath))
                {
                    var res = new HttpErrorDto
                    {
                        ErrorCode = 404,
                    };
                    return (res, null);
                }
                if (dto.Password is not null)
                {
                    if (password is null || !VerifyPassword(password, dto.Password))
                    {
                        var res = new HttpErrorDto
                        {
                            ErrorCode = 403,
                            Message = "bad password",
                        };
                        return (res, null);
                    }
                }
                if (dto.ExpireTime is not null)
                {
                    if (dto.ExpireTime < (ulong)DateTime.UtcNow.Ticks)
                    {
                        var res = new HttpErrorDto
                        {
                            ErrorCode = 410,
                            Message = "expired share",
                        };
                        return (res, null);
                    }
                }
                return (null, new FileStream(filepath, FileMode.Open, FileAccess.Read));
            }
            catch (HttpErrorException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to download share",
                });
            }
        }

        public async Task<bool> CloseShareAsync(MemberDto member, string token)
        {
            try
            {
                var sql = "UPDATE share " +
                    "SET expire_time = 0 " +
                    "WHERE member_id = @Id AND token = UUID_TO_BIN(@Token)";
                using var conn = _connService.Connection;
                return await conn.ExecuteAsync(sql, new
                {
                    Id = member.Id,
                    Token = token,
                }) != 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to close share",
                });
            }
        }

        public async Task<bool> UpdateShareAsync(ShareUpdateDto dto, string token, MemberDto member)
        {
            try
            {
                var sql = "UPDATE share " +
                "SET password = @Password, expire_time = @Expire, comment = @Comment, share_name = @ShareName " +
                "WHERE member_id = @Id AND token = UUID_TO_BIN(@Token)";
                var password = dto.Password;
                if (password is not null)
                {
                    password = EncryptPassword(password);
                }
                using var conn = _connService.Connection;
                return await conn.ExecuteAsync(sql, new
                {
                    Password = password,
                    Expire = dto.ExpireTime ?? (ulong)DateTime.MaxValue.Ticks,
                    Comment = dto.Comment,
                    ShareName = dto.ShareName,
                    Token = token,
                    Id = member.Id,
                }) != 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to update share",
                });
            }

        }

        public async Task DeleteShareAsync(string target, MemberDto member)
        {
            try
            {
                var sql = "DELETE FROM share " +
                    "WHERE member_id = @Id AND target = @Target";
                using var conn = _connService.Connection;
                await conn.ExecuteAsync(sql, new
                {
                    Target = target,
                    Id = member.Id,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to delete share",
                });
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <returns>404 : NotFound Share</returns>
        /// <exception cref="HttpErrorException"></exception>
        public async Task<(HttpErrorDto? err, bool? result)> ValidatePassword(string password, string token)
        {
            try
            {
                var sql = "Select password FROM share WHERE token = UUID_TO_BIN(@Token)";
                using var conn = _connService.Connection;
                var hash = await conn.QueryFirstOrDefaultAsync<string>(sql, new
                {
                    Token = token,
                });
                if (hash is null)
                {
                    var err = new HttpErrorDto()
                    {
                        ErrorCode = 404,
                    };
                    return (err, null);
                }
                return (null, VerifyPassword(password, hash));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to Validate Password",
                });
            }
        }

        public async Task<bool> CheckPassword(string token)
        {
            try
            {
                var sql = "Select password FROM share WHERE token = UUID_TO_BIN(@Token)";
                using var conn = _connService.Connection;
                var hash = await conn.QueryFirstOrDefaultAsync<string>(sql, new
                {
                    Token = token,
                });
                return hash is not null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDto
                {
                    ErrorCode = 500,
                    Message = "fail to check Password",
                });
            }
        }
    }
}
