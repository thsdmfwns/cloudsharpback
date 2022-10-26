namespace cloudsharpback.Models
{
    public class ShareRequestDto
    {
        public ShareRequestDto(string target, string? password, ulong? expireTime, string? comment, string? shareName)
        {
            Target = target;
            Password = password;
            ExpireTime = expireTime;
            Comment = comment;
            ShareName = shareName;
        }

        public string Target { get; set; }
        public string? Password { get; set; }
        public ulong? ExpireTime { get; set; }
        public string? Comment { get; set; }
        public string? ShareName { get; set; }
    }
}
