using JsonWebToken;

namespace cloudsharpback.Models
{
    public class MemberDto
    {
        public MemberDto(ulong id, ulong role, string email, string nickname, string directory, string? profileImage)
        {
            Id = id;
            Role = role;
            Email = email;
            Nickname = nickname;
            Directory = directory;
            ProfileImage = profileImage;
        }

        public ulong Id { get; set; }
        public ulong Role { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string Directory { get; set; }
        public string? ProfileImage { get; set; }

        public static MemberDto ParseToken(Jwt jwt)
        {
            var nickname = jwt.Payload!["nickname"] ?? throw new NullReferenceException();
            var email = jwt.Payload["email"] ?? throw new NullReferenceException();
            var userId = jwt.Payload["userId"] ?? throw new NullReferenceException();
            var roleId = jwt.Payload["roleId"] ?? throw new NullReferenceException();
            var directory = jwt.Payload["directory"] ?? throw new NullReferenceException();
            var profileImage = jwt.Payload["profile_image"];
            return new MemberDto(
                id: ulong.Parse((string)userId),
                role: ulong.Parse((string)roleId),
                email : (string)email,
                nickname : (string)nickname,
                directory: (string)directory,
                profileImage: (string?)profileImage
                );
        }
    }
}
