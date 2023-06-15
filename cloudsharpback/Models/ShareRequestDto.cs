namespace cloudsharpback.Models
{
    public class ShareRequestDto
    {
        public required string Target { get; set; }
        public string? Password { get; set; }
        public ulong? ExpireTime { get; set; }
        public string? Comment { get; set; }
        public string? ShareName { get; set; }
    }
}
