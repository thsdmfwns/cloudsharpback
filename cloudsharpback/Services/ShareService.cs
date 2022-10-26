using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
using Dapper;
using Org.BouncyCastle.Ocsp;
using System.Diagnostics.Metrics;
using System.IO;

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
        public async Task<string?> Share(MemberDto member, ShareRequestDto req)
        {
            try
            {
                var filepath = Path.Combine(userPath(member.Directory), req.Target);
                if (!FileExist(filepath))
                {
                    throw new HttpErrorException(new HttpErrorDetail
                    {
                        ErrorCode = 404,
                        Message = $"Fail to Find Sharing Object => {req.Target}",
                    });
                }
                var password = req.Password;
                if (password is not null)
                {
                    password = EncryptPassword(password);
                }
                var sql = "INSERT INTO share(member_id, target, password, expire_time, comment, share_time, share_name, token) " +
                    "VALUES(@MemberID, @Target, @Password, @ExpireTime, @Comment, @ShareTime, @ShareName, UUID_TO_BIN(@Token))";
                using var conn = _connService.Connection;
                var token = Guid.NewGuid().ToString();
                await conn.ExecuteAsync(sql, new
                {
                    MemberId = member.Id,
                    Target = req.Target,
                    Password = password,
                    ExpireTime = req.ExpireTime,
                    Comment = req.Comment,
                    ShareTime = DateTime.UtcNow.Ticks,
                    ShareName = req.ShareName,
                    Token = token,
                });
                return token;
            }
            catch (HttpErrorException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDetail
                {
                    ErrorCode = 500,
                    Message = "fail to sharing",
                });
            }
        }

        public async Task<ShareResponseDto?> GetShareAsync(string token)
        {
            try
            {
                //ulong id, ulong ownerId, string ownerNick, ulong shareTime, ulong? expireTime, string target, string? shareName, string? comment
                var sql = "Select m.member_id ownerId, m.nickname ownerNick, " +
                    "s.share_time shareTime, s.expire_time expireTime, s.target target, s.share_name shareName, s.comment, BIN_TO_UUID(s.token) token " +
                    "FROM share AS s " +
                    "INNER JOIN member AS m " +
                    "ON s.member_id = m.member_id " +
                    "WHERE s.token = UUID_TO_BIN(@Token)";
                using var conn = _connService.Connection;
                return await conn.QueryFirstOrDefaultAsync<ShareResponseDto>(sql, new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDetail
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
                //ulong id, ulong ownerId, string ownerNick, ulong shareTime, ulong? expireTime, string target, string? shareName, string? comment
                var sql = "Select m.member_id ownerId, m.nickname ownerNick, " +
                    "s.share_time shareTime, s.expire_time expireTime, s.target target, s.share_name shareName, s.comment, BIN_TO_UUID(s.token) token " +
                    "FROM share AS s " +
                    "INNER JOIN member AS m " +
                    "ON s.member_id = m.member_id " +
                    "WHERE s.member_id = @ID";
                using var conn = _connService.Connection;
                var result = await conn.QueryAsync<ShareResponseDto>(sql, new { ID = member.Id });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDetail
                {
                    ErrorCode = 500,
                    Message = "fail to get share",
                });
            }
        }

        public async Task<FileStream> DownloadShareAsync(string token, string? password)
        {
            try
            {
                if (!Guid.TryParse(token, out _))
                {
                    throw new HttpErrorException(new HttpErrorDetail
                    {
                        ErrorCode = 400,
                        Message = "token is not correct format",
                    });
                }
                var sql = "Select BIN_TO_UUID(m.directory) directory , s.target, s.expire_time expireTime, s.password " +
                    "FROM share AS s " +
                    "INNER JOIN member AS m " +
                    "ON s.member_id = m.member_id " +
                    "WHERE s.token = UUID_TO_BIN(@Token)";
                using var conn = _connService.Connection;
                var result = await conn.QueryFirstOrDefaultAsync<ShareDownloadDto>(sql, new { Token = token });
                var filepath = Path.Combine(userPath(result.Directory), result.Target);
                if (result is null || !FileExist(filepath))
                {
                    throw new HttpErrorException(new HttpErrorDetail
                    {
                        ErrorCode = 404,
                        Message = "Fail to Find Sharing Object",
                    });
                }
                if (result.Password is not null)
                {
                    if (password is null || !VerifyPassword(password, result.Password))
                    {
                        throw new HttpErrorException(new HttpErrorDetail
                        {
                            ErrorCode = 403,
                            Message = "Bad Password",
                        });
                    }
                }
                if (result.ExpireTime is not null)
                {
                    if (result.ExpireTime < (ulong)DateTime.UtcNow.Ticks)
                    {
                        throw new HttpErrorException(new HttpErrorDetail
                        {
                            ErrorCode = 410,
                            Message = "expired share",
                        });
                    }
                }
                return new FileStream(filepath, FileMode.Open, FileAccess.Read);
            }
            catch (HttpErrorException) { throw; }
            catch (Exception ex) 
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpErrorDetail
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
                throw new HttpErrorException(new HttpErrorDetail
                {
                    ErrorCode = 500,
                    Message = "fail to close share",
                });
            }
        }
    }
}
