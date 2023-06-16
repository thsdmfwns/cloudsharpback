using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
using System.Net.Mail;
using cloudsharpback.Repository.Interface;

namespace cloudsharpback.Services
{
    public class MemberService : IMemberService
    {
        private readonly ILogger _logger;
        private readonly string _profilePath;
        private readonly IMemberRepository _memberRepository;
        public MemberService(ILogger<IMemberService> logger, IPathStore pathStore, IMemberRepository memberRepository)
        {
            _profilePath = pathStore.ProfilePath;
            if (!Directory.Exists(_profilePath)) Directory.CreateDirectory(_profilePath);
            _logger = logger;
            _memberRepository = memberRepository;
        }

        /// <returns>404 : fail to login</returns>
        public async Task<(HttpResponseDto? err, MemberDto? result)> GetMemberById(ulong id)
        {
            try
            {
                var res = await _memberRepository.GetMemberById(id);
                return res is null
                    ? (new HttpResponseDto() { HttpCode = 404, Message = "member not found" }, null)
                    : (null, res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to get member",
                });
            }
        }

        /// <returns>415 : bad type, 409 : try again, 404: member not found</returns>
        public async Task<HttpResponseDto?> UploadProfileImage(IFormFile imageFile, MemberDto member)
        {
            try
            {
                var profileId = Guid.NewGuid();
                var extension = Path.GetExtension(imageFile.FileName);
                var mime = MimeTypeUtil.GetMimeType(extension);
                if (mime is null 
                    || mime.Split('/')[0] != "image")
                {
                    return new HttpResponseDto() { HttpCode = 415, Message = "bad type" };
                }
                var filename = profileId + extension;
                var filepath = Path.Combine(_profilePath, filename);
                if (File.Exists(filepath))
                {
                    return new HttpResponseDto() { HttpCode = 409, Message = "try again" };
                }
                using (var stream = File.Create(filepath))
                {
                    await imageFile.CopyToAsync(stream);
                }
                var res = await _memberRepository.TryUpdateMemberProfileImage(member.Id, filename);
                return res ? null : new HttpResponseDto() { HttpCode = 404, Message = "member not found" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to upload profile image",
                });
            }
        }

        public HttpResponseDto? DownloadProfileImage(string profileImage, out FileStream? fileStream, out string? contentType)
        {
            try
            {
                fileStream = null;
                var filepath = Path.Combine(_profilePath, profileImage);
                contentType = MimeTypeUtil.GetMimeType(profileImage);
                if (!File.Exists(filepath)
                    || contentType is null)
                {
                    return new HttpResponseDto() { HttpCode = 404, Message = "file not found" };
                }
                fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to download file",
                });
            }
        }

        public async Task<HttpResponseDto?> UpadteNickname(MemberDto member, string changeNick)
        {
            try
            {
                if (member.Nickname.Equals(changeNick))
                {
                    return null;
                }
                var res = await _memberRepository.TryUpdateMemberNickname(member.Id, changeNick);
                return res ? null : new HttpResponseDto() { HttpCode = 404, Message = "member not found" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to update nick",
                });
            }
        }

        public async Task<HttpResponseDto?> UpadteEmail(MemberDto member, string changeEmail)
        {
            try
            {
                // validate email
                try
                {
                    var _ = new MailAddress(changeEmail);
                }
                catch (Exception)
                {
                    return new HttpResponseDto() { HttpCode = 400, Message = "bad email" };
                }
                var res = await _memberRepository.TryUpdateMemberEmail(member.Id, changeEmail);
                return res ? null : new HttpResponseDto() { HttpCode = 404, Message = "member not found" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to update email",
                });
            }
        }

        public async Task<(HttpResponseDto? err, bool result)> CheckPassword(MemberDto member, string password)
        {
            try
            {
                var passwordHash = await _memberRepository.GetMemberPasswordHashById(member.Id);
                return passwordHash is null 
                    ? (new HttpResponseDto() { HttpCode = 404, Message = "member not found" }, false)
                    : (null, PasswordEncrypt.VerifyPassword(password, passwordHash));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to check password",
                });
            }

        }

        public async Task<HttpResponseDto?> UpdatePassword(MemberDto member, UpadtePasswordDto requset)
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
                    return new HttpResponseDto() { HttpCode = 400, Message = "check password" };
                }
                
                var password = PasswordEncrypt.EncryptPassword(requset.ChangeTo);
                var result = await _memberRepository.TryUpdateMemberPassword(member.Id, password);
                return result ? null : new HttpResponseDto() { HttpCode = 404, Message = "member not found" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to update password",
                });
            }
        }
    }
}
