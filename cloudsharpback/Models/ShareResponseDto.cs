namespace cloudsharpback.Models
{
    public class ShareResponseDto
    {
        public required ulong OwnerId { get; set; }
        public required string OwnerNick { get; set; }
        public required ulong ShareTime { get; set; }
        public ulong? ExpireTime { get; set; }
        public required ulong FileSize { get; set; }
        public required string Target { get; set; }
        public string? ShareName { get; set; }
        public string? Comment { get; set; }
        public required string Token { get; set; }
        public required bool HasPassword { get; set; }
    }
}
