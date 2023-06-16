using cloudsharpback.Models;
using cloudsharpback.Repository.Interface;
using cloudsharpback.Services.Interfaces;
using cloudsharpback.Utills;
using Dapper;

namespace cloudsharpback.Services
{
    public class UserService : IUserService
    {
        private readonly IMemberRepository _memberRepository;
        private readonly ILogger _logger;

        public UserService(ILogger<IUserService> logger, IMemberRepository memberRepository)
        {
            _logger = logger;
            _memberRepository = memberRepository;
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
        /// <returns>404 : bad json </returns>
        /// <exception cref="HttpErrorException"></exception>
        public async Task<HttpResponseDto?> Register(RegisterDto registerDto, ulong role)
        {
            try
            {
                registerDto.Pw = PasswordEncrypt.EncryptPassword(registerDto.Pw);
                var result = await _memberRepository.TryAddMember(registerDto, role);
                return result ? null : new HttpResponseDto() { HttpCode = 400 };
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

    }
}
