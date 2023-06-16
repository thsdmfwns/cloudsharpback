namespace cloudsharpback.Models
{
    public class ShareDownloadDto
    {
        public required string Directory { get; set; }
        public required string Target { get; set; }
        public string? Password { get; set; }
        public ulong? ExpireTime { get; set; }
    }
}
