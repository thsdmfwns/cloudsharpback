using JsonWebToken;

namespace cloudsharpback.Models
{
    public class MemberDto
    {
        public MemberDto(ulong member_id, ulong role_id, string email, string nickname)
        {
            Id = member_id;
            Role = role_id;
            Email = email;
            Nickname = nickname;
        }

        public ulong Id { get; set; }
        public ulong Role { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public static MemberDto ParseToken(Jwt jwt)
        {
                var nickname = jwt.Payload["nickname"] ?? throw new NullReferenceException();
                var email = jwt.Payload["email"] ?? throw new NullReferenceException();
                var userId = jwt.Payload["userId"] ?? throw new NullReferenceException();
                var roleId = jwt.Payload["roleId"] ?? throw new NullReferenceException();
                return new MemberDto(
                    member_id : ulong.Parse((string)userId),
                    role_id : ulong.Parse((string)roleId),
                    email : (string)email,
                    nickname : (string)nickname);
        }
    }
}
