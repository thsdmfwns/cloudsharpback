using cloudsharpback.Models;
using cloudsharpback.Services.Interfaces;
using JsonWebToken;
using Microsoft.Extensions.Logging;

namespace cloudsharpback.Services
{
    public class JWTService : IJWTService
    {
        private readonly SymmetricJwk jwtKey;
        private readonly ILogger _logger;

        public JWTService(IConfiguration configuration, ILogger<IJWTService> logger)
        {
            jwtKey = new SymmetricJwk(configuration["JWT:key"], SignatureAlgorithm.HmacSha512);
            _logger = logger;
        }

        public string WriteToken(MemberDto data)
        {
            try
            {
                var descriptor = new JwsDescriptor()
                {
                    Algorithm = SignatureAlgorithm.HmacSha512,
                    SigningKey = jwtKey,
                    IssuedAt = DateTime.UtcNow,
                    ExpirationTime = DateTime.UtcNow.AddDays(1),
                };
                descriptor.AddClaim("nickname", data.Nickname);
                descriptor.AddClaim("email", data.Email);
                descriptor.AddClaim("userId", data.Id.ToString());
                descriptor.AddClaim("roleId", data.Role.ToString());
                descriptor.AddClaim("directory", data.Directory);
                return new JwtWriter().WriteTokenString(descriptor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
                throw new HttpErrorException(new ServiceResult
                {
                    ErrorCode = 500,
                    Message = "fail to write token",
                });
            }
        }

        public bool TryValidateToken(string token, out MemberDto? member)
        {
            try
            {
                var policy = new TokenValidationPolicyBuilder()
                .RequireSignature(jwtKey, SignatureAlgorithm.HmacSha512)
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
                throw new HttpErrorException(new ServiceResult
                {
                    ErrorCode = 500,
                    Message = "fail to validate token",
                });
            }
        }
    }
}
