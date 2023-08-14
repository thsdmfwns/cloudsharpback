using System.Security.Cryptography;
using System.Text;
using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Services.Interfaces;
using JsonWebToken;
using Org.BouncyCastle.Utilities.Encoders;

namespace cloudsharpback.Services
{
    public class JWTService : IJWTService
    {
        private readonly SymmetricJwk _jwtKey;
        private readonly ILogger _logger;

        public JWTService(IConfiguration configuration, ILogger<IJWTService> logger)
        {
            string key;
            using (var sha512 = SHA512.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
                var hashBytes = sha512.ComputeHash(inputBytes);
                key = Encoding.UTF8.GetString(Base64.Encode(hashBytes));
            }
            _jwtKey = new SymmetricJwk(key, SignatureAlgorithm.HmacSha512);
            _logger = logger;
        }

        public string WriteAccessToken(MemberDto data)
        {
            try
            {
                var descriptor = new JwsDescriptor()
                {
                    Algorithm = SignatureAlgorithm.HmacSha512,
                    SigningKey = _jwtKey,
                    IssuedAt = DateTime.UtcNow,
                    ExpirationTime = DateTime.UtcNow.AddDays(1),
                };
                descriptor.AddClaim("nickname", data.Nickname);
                descriptor.AddClaim("email", data.Email);
                descriptor.AddClaim("userId", data.Id.ToString());
                descriptor.AddClaim("roleId", data.Role.ToString());
                descriptor.AddClaim("directory", data.Directory);
                if (data.ProfileImage is not null)
                {
                    descriptor.AddClaim("profile_image", data.ProfileImage);
                }
                return new JwtWriter().WriteTokenString(descriptor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to write ac token",
                });
            }
        }

        string IJWTService.WriteRefreshToken(MemberDto data)
        {
            try
            {
                var descriptor = new JwsDescriptor()
                {
                    Algorithm = SignatureAlgorithm.HmacSha512,
                    SigningKey = _jwtKey,
                    IssuedAt = DateTime.UtcNow,
                    ExpirationTime = DateTime.UtcNow.AddDays(30),
                };
                descriptor.AddClaim("userId", data.Id.ToString());
                return new JwtWriter().WriteTokenString(descriptor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to write rf token",
                });
            }
        }


        public bool TryValidateAccessToken(string token, out MemberDto? member)
        {
            try
            {
                var policy = new TokenValidationPolicyBuilder()
                .RequireSignature(_jwtKey, SignatureAlgorithm.HmacSha512)
                .EnableLifetimeValidation()
                .Build();
                var reader = new JwtReader();
                var result = reader.TryReadToken(token, policy);
                if (result.Token == null 
                    || result.Status != TokenValidationStatus.Success)
                {
                    member = null;
                    return false;
                }
                member = MemberDto.ParseToken(result.Token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to validate token",
                });
            }
        }

        public bool TryValidateRefreshToken(string token, out ulong? memberId)
        {
            try
            {
                memberId = null;
                var policy = new TokenValidationPolicyBuilder()
                .RequireSignature(_jwtKey, SignatureAlgorithm.HmacSha512)
                .EnableLifetimeValidation()
                .Build();
                var reader = new JwtReader();
                var result = reader.TryReadToken(token, policy);
                if (result.Token == null
                    || result.Status != TokenValidationStatus.Success)
                {
                    return false;
                }
                var memberIdObj = result.Token.Payload!["userId"];
                if (memberIdObj is null 
                    || !ulong.TryParse((string)memberIdObj, out var id))
                {
                    return false;
                }
                memberId = id;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new HttpResponseDto
                {
                    HttpCode = 500,
                    Message = "fail to validate token",
                });
            }
        }
    }
}
