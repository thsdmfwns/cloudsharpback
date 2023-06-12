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
        public string? Path { get; set; }

        public static FileDto FromDirectoryInfo(DirectoryInfo directoryInfo, string memberDirectoryPath)
        {
            return new()
            {
                Name = directoryInfo.Name,
                FileType = Models.FileType.FOLDER,
                Path = directoryInfo.FullName.Substring(directoryInfo.FullName.Last() == '/'
                    ? memberDirectoryPath.Length + 1
                    : memberDirectoryPath.Length)
            };
        }

        public static FileDto FromFileInfo(FileInfo fileInfo, string memberDirectoryPath)
            => new() {
                Name = fileInfo.Name,
                FileType = Models.FileType.FILE,
                Extention = fileInfo.Extension,
                LastWriteTime = fileInfo.LastWriteTime.ToUniversalTime().Ticks,
                Size = (ulong?)fileInfo.Length,
                Path = fileInfo.FullName.Substring(memberDirectoryPath.Length + 1),
            };
    }
}
