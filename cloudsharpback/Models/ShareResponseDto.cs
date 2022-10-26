namespace cloudsharpback.Models
{
    public class ShareResponseDto
    {
        public ShareResponseDto(ulong ownerId, string ownerNick, ulong shareTime, ulong? expireTime, string target, string? shareName, string? comment, string token)
        {
            OwnerId = ownerId;
            OwnerNick = ownerNick;
            ShareTime = shareTime;
            ExpireTime = expireTime;
            Target = target;
            ShareName = shareName;
            Comment = comment;
            Token = token;
        }
        public ulong OwnerId { get; set; }
        public string OwnerNick { get; set; }
        public ulong ShareTime { get; set; }
        public ulong? ExpireTime { get; set; }
        public string Target { get; set; }
        public string? ShareName { get; set; }
        public string? Comment { get; set; }
        public string Token { get; set; }
    }
}
