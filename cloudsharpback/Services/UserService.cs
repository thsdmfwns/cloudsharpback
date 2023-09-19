using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utils;
using Dapper;

namespace cloudsharpback.Services
{
    public class UserService : IUserService
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IPathStore _pathStore;
        private readonly ILogger _logger;

        public UserService(ILogger<IUserService> logger, IMemberRepository memberRepository, IPathStore pathStore)
        {
            _logger = logger;
            _memberRepository = memberRepository;
            _pathStore = pathStore;
        }

        private async Task<string?> GetPasswordHash(string id)
            => await _memberRepository.GetMemberPasswordHashByLoginId(id);

        public async Task<bool> IdCheck(string id)
            => await _memberRepository.TryLoginIdDuplicate(id);

        public async Task<(HttpResponseDto? err, MemberDto? result)> Login(LoginDto loginDto)
        {
            try
            {
                var passwordHash = await GetPasswordHash(loginDto.Id);
                if (passwordHash is null
                    || !PasswordEncrypt.VerifyPassword(loginDto.Password, passwordHash))
                {
                    var res = new HttpResponseDto() { HttpCode = 401, Message = "login fail" };
                    return (res, null);
                }
                var result = await _memberRepository.GetMemberByLoginId(loginDto.Id);
                return result is null
                    ? (new HttpResponseDto() { HttpCode = 404, Message = "member not found" }, null)
                    : (null, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to login",
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="registerDto"></param>
        /// <param name="role"></param>
        /// <param name="directoryId"></param>
        /// <returns>404 : bad json </returns>
        /// <exception cref="HttpErrorException"></exception>
        public async Task<HttpResponseDto?> Register(RegisterDto registerDto, ulong role, Guid? directoryId = null)
        {
            try
            {
                var password = PasswordEncrypt.EncryptPassword(registerDto.Pw);
                directoryId ??= Guid.NewGuid();
                var result = await _memberRepository.TryAddMember(
                    registerDto.Id,
                    password,
                    registerDto.Nick,
                    registerDto.Email,
                    directoryId.Value,
                    role,
                    null);
                if (!result) return new HttpResponseDto() { HttpCode = 400 };
                MakeBaseDirectory(directoryId.Value);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to register",
                });
            }
        }
        private void MakeBaseDirectory(Guid memberDirectoryId)
        {
            var dir = _pathStore.MemberDirectory(memberDirectoryId.ToString());
            string SubPath(string foldername) => Path.Combine(dir, foldername);
            Directory.CreateDirectory(SubPath("Download"));
            Directory.CreateDirectory(SubPath("Music"));
            Directory.CreateDirectory(SubPath("Video"));
            Directory.CreateDirectory(SubPath("Document"));
        }
    }
}
