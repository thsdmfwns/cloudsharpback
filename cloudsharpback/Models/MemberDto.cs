namespace cloudsharpback.Models
{
    public class MemberDto
    {
        public MemberDto(ulong id, int role, string email, string nickname)
        {
            Id = id;
            Role = role;
            Email = email;
            Nickname = nickname;
        }

        public ulong Id { get; set; }
        public int Role { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
    }
}
