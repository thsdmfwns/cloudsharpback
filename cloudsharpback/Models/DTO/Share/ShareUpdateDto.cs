namespace cloudsharpback.Models.DTO.Share
{
    public class ShareUpdateDto
    {
        public string? Password { get; set; }
        public ulong? ExpireTime { get; set; }
        public string? Comment { get; set; }
        public string? ShareName { get; set; }
    }
}
