using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Models.DTO.Share;
using cloudsharpback.Models.Ticket;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utils;

namespace cloudsharpback.Services
{
    public class ShareService : IShareService
    {
        private readonly IShareRepository _shareRepository;
        private readonly ILogger _logger;
        private readonly IPathStore _pathStore;

        public ShareService(ILogger<IShareService> logger, IPathStore pathStore, IShareRepository shareRepository)
        {
            _pathStore = pathStore;
            _shareRepository = shareRepository;
            _logger = logger;
        }
        
        private string GetMemberTargetPath(MemberDto member, string? targetPath) =>
            GetMemberTargetPath(member.Directory, targetPath);
        
        private string GetMemberTargetPath(string memberDirectoryId, string? targetPath) =>
            _pathStore.GetMemberTargetPath(memberDirectoryId, targetPath);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="req"></param>
        /// <param name="token"></param>
        /// <returns>404 : no file for share</returns>
        /// <exception cref="HttpErrorException"></exception>
        public async Task<HttpResponseDto?> Share(MemberDto member, ShareRequestDto req, Guid? token = null)
        {
            try
            {
                token ??= Guid.NewGuid();
                var filepath = GetMemberTargetPath(member, req.Target);
                var fileinfo = new FileInfo(filepath);
                if (!fileinfo.Exists)
                {
                    return new HttpResponseDto
                    {
                        HttpCode = 404,
                        Message = $"no file for share",
                    };
                }
                var res = await _shareRepository.TryAddShare(
                    member.Id,
                    req.Target,
                    req.Password is not null 
                        ? PasswordEncrypt.EncryptPassword(req.Password) : null,
                    req.ExpireTime ?? (ulong)DateTime.MaxValue.Ticks,
                    req.Comment,
                    req.ShareName,
                    token.Value,
                    (ulong)fileinfo.Length
                    );
                return res ? null : new HttpResponseDto(){HttpCode = 400};
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
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
        public async Task<(HttpResponseDto? err, ShareResponseDto? result)> GetShareAsync(Guid token)
        {
            try
            {
                var res = await _shareRepository.GetShareByToken(token);
                if (res is null)
                {
                    return (new HttpResponseDto() { HttpCode = 404, Message = "Share Not Found" }, null);
                } 
                if (res.ExpireTime < (ulong)DateTime.UtcNow.Ticks)
                {
                    return (new HttpResponseDto()
                    {
                        HttpCode = 410,
                        Message = "expired share",
                    }, null);
                }
                return (null, res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to get shares",
                });
            }
        }

        public async Task<List<ShareResponseDto>> GetSharesAsync(MemberDto member)
        {
            try
            {
                return await _shareRepository.GetSharesListByMemberId(member.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to get share",
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="memberDto"></param>
        /// <returns>404 : file doesnt exist , 403 : bad password, 410 : expired share</returns>
        /// <exception cref="HttpErrorException"></exception>
        public async Task<(HttpResponseDto? err, DownloadTicket? ticket)> GetDownloadTicketValue(
            ShareDowonloadRequestDto req, MemberDto? memberDto)
        {
            try
            {
                if (!Guid.TryParse(req.Token, out var token))
                {
                    return (new HttpResponseDto() { HttpCode = 400 }, null);
                }

                var dto = await _shareRepository.GetShareDownloadDtoByToken(token);
                if (dto is null)
                {
                    var res = new HttpResponseDto
                    {
                        HttpCode = 404,
                        Message = "share not found"
                    };
                    return (res, null);
                }

                if (dto.Password is not null
                    && (req.Password is null
                        || !PasswordEncrypt.VerifyPassword(req.Password, dto.Password)))
                {
                    var res = new HttpResponseDto
                    {
                        HttpCode = 403,
                        Message = "bad password",
                    };
                    return (res, null);
                }

                if (dto.ExpireTime is not null
                    && dto.ExpireTime < (ulong)DateTime.UtcNow.Ticks)
                {
                    return (new HttpResponseDto
                    {
                        HttpCode = 410,
                        Message = "expired share",
                    }, null);
                }

                var filePath = GetMemberTargetPath(dto.Directory, dto.Target);
                if (!File.Exists(filePath))
                {
                    return (new HttpResponseDto
                    {
                        HttpCode = 404,
                        Message = "File Notfound",
                    }, null);
                }

                var val = new DownloadTicket()
                {
                    FileDownloadType = FileDownloadType.Download,
                    TargetFilePath = filePath,
                    Owner = memberDto
                };
                return (null, val);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to download share",
                });
            }
        }
        public async Task<HttpResponseDto?> CloseShareAsync(MemberDto member, Guid token)
        {
            try
            {
                var res = await _shareRepository.TrySetShareExpireTimeToZero(member.Id, token);
                return res ? null : new HttpResponseDto{HttpCode = 404, Message = "Share not found"};
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to close share",
                });
            }
        }

        public async Task<HttpResponseDto?> UpdateShareAsync(ShareUpdateDto dto, Guid token, MemberDto member)
        {
            try
            {
                var password = dto.Password;
                if (password is not null)
                {
                    password = PasswordEncrypt.EncryptPassword(password);
                }
                var res = await _shareRepository.TryUpdateShare(member.Id, token, password, dto.Comment, dto.ExpireTime, dto.ShareName);
                return res ? null : new HttpResponseDto{HttpCode = 404, Message = "share not found"};
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to update share",
                });
            }

        }

        public async Task<bool> CheckExistShareByTargetPath(string target, MemberDto member)
        {
            try
            {
                return (await _shareRepository.GetSharesByTargetFilePath(member.Id, target)).Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to check exist share by file path",
                });
            }
        }

        public async Task<(HttpResponseDto? err, List<ShareResponseDto> shares)> FindSharesInDirectory(MemberDto memberDto, string targetDir)
        {
            try
            {
                if (!Directory.Exists(GetMemberTargetPath(memberDto, targetDir)))
                {
                    return (new HttpResponseDto() { HttpCode = 404, Message = "Directory Not Found" }, new List<ShareResponseDto>());
                }

                var res = await _shareRepository.GetSharesInDirectory(memberDto.Id, targetDir);
                return (null, res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to find share",
                });
            }
        }

        public async Task<HttpResponseDto?> DeleteShareAsync(string target, MemberDto member)
        {
            try
            {
                if (!(await _shareRepository.GetSharesByTargetFilePath(member.Id, target)).Any())
                {
                    return null;
                }
                var res = await _shareRepository.TryDeleteShare(member.Id, target);
                return res ? null : new HttpResponseDto { HttpCode = 404, Message = "share not found" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to delete share",
                });
            }
        }

        public async Task<HttpResponseDto?> DeleteSharesInDirectory(MemberDto memberDto, string targetDirectoryPath)
        {
            var targetDirPath = GetMemberTargetPath(memberDto, targetDirectoryPath);
            var targetdir = new DirectoryInfo(targetDirPath);
            if (!targetdir.Exists)
            {
                
                
                return new HttpResponseDto() { HttpCode = 404, Message = "Directory not found" };
            }
            var shares = await _shareRepository.GetSharesInDirectory(memberDto.Id, targetDirectoryPath);
            if (shares.Count < 1)
            {
                return null;
            }
            var res = await _shareRepository.TryDeleteShareInDirectory(memberDto.Id, targetDirectoryPath);
            return res == shares.Count ? null : new HttpResponseDto() { HttpCode = 404, Message = "few shares not found"};
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <returns>404 : NotFound Share</returns>
        /// <exception cref="HttpErrorException"></exception>
        public async Task<(HttpResponseDto? err, bool? result)> ValidatePassword(string password, Guid token)
        {
            try
            {
                var hash = await _shareRepository.GetPasswordHashByToken(token);
                if (hash is null)
                {
                    var err = new HttpResponseDto()
                    {
                        HttpCode = 404,
                    };
                    return (err, null);
                }
                return (null, PasswordEncrypt.VerifyPassword(password, hash));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to Validate Password",
                });
            }
        }

        public async Task<bool> CheckPassword(Guid token)
        {
            try
            {
                var hash = await _shareRepository.GetPasswordHashByToken(token);
                return hash is not null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to check Password",
                });
            }
        }
    }
}
