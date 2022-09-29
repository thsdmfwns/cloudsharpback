namespace cloudsharpback.Models
{
    public class RoleDto
    {
        public RoleDto(ulong role_id, string name)
        {
            Id = role_id;
            Name = name;
        }

        public ulong Id { get; set; }
        public string Name { get; set; }
    }
}
