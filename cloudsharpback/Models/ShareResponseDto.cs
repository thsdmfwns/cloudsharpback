namespace cloudsharpback.Models
{
    public class ShareResponseDto
    {
        //System.UInt64 ownerId, System.String ownerNick, System.UInt64 shareTime, System.UInt64 expireTime, System.String target, System.String shareName, System.String comment, System.String token, System.UInt64 filesize
        public ShareResponseDto(ulong ownerId, string ownerNick, ulong shareTime, ulong? expireTime,
            string target, string? shareName, string? comment, string token, string? password, ulong filesize)
        {
            OwnerId = ownerId;
            OwnerNick = ownerNick;
            ShareTime = shareTime;
            ExpireTime = expireTime;
            Target = target;
            ShareName = shareName;
            Comment = comment;
            Token = token;
            HasPassword = password is not null;
            FileSize = filesize;
        }
        public ulong OwnerId { get; set; }
        public string OwnerNick { get; set; }
        public ulong ShareTime { get; set; }
        public ulong? ExpireTime { get; set; }
        public ulong FileSize { get; set; }
        public string Target { get; set; }
        public string? ShareName { get; set; }
        public string? Comment { get; set; }
        public string Token { get; set; }
        public bool HasPassword { get; set; }
    }
}
