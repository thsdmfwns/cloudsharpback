using Transmission.API.RPC.Entity;

namespace cloudsharpback.Models.DTO.FIle
{
    public class TorrentInfoDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public long TotalSize { get; set; }
        public double Percent { get; set; }
        public int PeersCount { get; set; }
        public long DownLoadSpeed { get; set; }
        public long UploadSpeed { get; set; }
        public int ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public int ETA { get; set; }
        public string? Hash { get; set; }

        static public TorrentInfoDto FromTransMission(TorrentInfo info)
        {
            return new()
            {
                Id = info.ID,
                Name = info.Name,
                TotalSize = info.TotalSize,
                Percent = info.PercentDone,
                PeersCount = info.PeersConnected,
                DownLoadSpeed = info.RateDownload,
                UploadSpeed = info.RateUpload,
                ErrorCode = info.Error,
                ErrorMessage = info.ErrorString,
                ETA = info.ETA,
                Hash = info.HashString,
            };
        }

    }

}
