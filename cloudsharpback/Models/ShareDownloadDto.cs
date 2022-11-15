namespace cloudsharpback.Models
{
    public class ShareDownloadDto
    {
        public ShareDownloadDto(string directory, string target, ulong? expireTime, string? password)
        {
            Directory = directory;
            Target = target;
            ExpireTime = expireTime;
            Password = password;
        }

        public string Directory { get; set; }
        public string Target { get; set; }
        public string? Password { get; set; }
        public ulong? ExpireTime { get; set; }
        public DateTime DtoExpireTime = DateTime.Now.AddMinutes(1);
    }
}
