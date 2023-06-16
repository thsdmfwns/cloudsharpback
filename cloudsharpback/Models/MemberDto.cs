using JsonWebToken;

namespace cloudsharpback.Models
{
    public class MemberDto
    {
        public required ulong Id { get; set; }
        public required ulong Role { get; set; }
        public required string Email { get; set; }
        public required string Nickname { get; set; }
        public required string Directory { get; set; }
        public string? ProfileImage { get; set; }

        public static MemberDto ParseToken(Jwt jwt)
        {
            var nickname = jwt.Payload!["nickname"] ?? throw new NullReferenceException();
            var email = jwt.Payload["email"] ?? throw new NullReferenceException();
            var userId = jwt.Payload["userId"] ?? throw new NullReferenceException();
            var roleId = jwt.Payload["roleId"] ?? throw new NullReferenceException();
            var directory = jwt.Payload["directory"] ?? throw new NullReferenceException();
            var profileImage = jwt.Payload["profile_image"];
            return new MemberDto
            {
                Id = ulong.Parse((string)userId),
                Role = ulong.Parse((string)roleId),
                Email = (string)email,
                Nickname = (string)nickname,
                Directory = (string)directory,
                ProfileImage = (string?)profileImage
            };
        }
    }
}
