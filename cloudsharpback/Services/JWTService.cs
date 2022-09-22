using cloudsharpback.Models;
using JsonWebToken;

namespace cloudsharpback.Services
{
    public class JWTService : IJWTService
    {
        private readonly SymmetricJwk jwtKey;

        public JWTService(IConfiguration configuration)
        {
            jwtKey = new SymmetricJwk(configuration["JWT:key"], SignatureAlgorithm.HmacSha512);
        }

        public string TokenCreate(MemberDto data)
        {
            var descriptor = new JwsDescriptor()
            {
                Algorithm = SignatureAlgorithm.HmacSha512,
                SigningKey = jwtKey,
                IssuedAt = DateTime.UtcNow,
                ExpirationTime = DateTime.UtcNow.AddDays(1),
                Issuer = "https://son-server.com",
                Audience = "https://son-server.com",
            };
            descriptor.AddClaim("NickName", data.Nickname);
            descriptor.AddClaim("Email", data.Email);
            descriptor.AddClaim("UserId", data.Id.ToString());

            var writer = new JwtWriter();
            var token = writer.WriteTokenString(descriptor);

            return token;
        }

        public bool TryTokenValidation(string token, out Jwt? jwt)
        {
            try
            {
                var policy = new TokenValidationPolicyBuilder()
                .RequireSignature(jwtKey, SignatureAlgorithm.HmacSha512)
                .RequireIssuer("https://son-server.com")
                .RequireAudience("https://son-server.com")
                .Build();
                var reader = new JwtReader();
                var result = reader.TryReadToken(token, policy);
                if (result.Token == null)
                {
                    jwt = null;
                    return false;
                }
                jwt = result.Token;
                return result.Status == TokenValidationStatus.Success;
            }
            catch (Exception)
            {
                jwt = null;
                return false;
            }
        }
    }
}
