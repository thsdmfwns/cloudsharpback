namespace cloudsharpback.Models.DTO.FIle
{
    public class YoutubeDlProgressDto
    {
        public YoutubeDlProgressDto(string percentage, string totalLength, string speed, string eTA)
        {
            Percentage = percentage;
            TotalLength = totalLength;
            Speed = speed;
            ETA = eTA;
        }

        public string Percentage { get; set; }
        public string TotalLength { get; set; }
        public string Speed { get; set; }
        public string ETA { get; set; }
    }
}
