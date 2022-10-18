namespace cloudsharpback.Models
{
    public enum FileType
    {
        FILE,
        FOLDER
    }

    public class FileDto
    {
        public string? Name { get; set; }
        public string? Extention { get; set; }
        public long? LastWriteTime { get; set; }
        public FileType? FileType { get; set; }
        public ulong? Size { get; set; }
    }
}
