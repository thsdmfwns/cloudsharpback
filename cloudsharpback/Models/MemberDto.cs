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
    }
}
