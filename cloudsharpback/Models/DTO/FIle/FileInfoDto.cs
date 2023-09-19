namespace cloudsharpback.Models.DTO.FIle
{
    public enum FileType
    {
        File,
        Folder
    }

    public class FileInfoDto
    {
        public required string Name { get; set; }
        public string? Extention { get; set; }
        public long? LastWriteTime { get; set; }
        public required FileType FileType { get; set; }
        public ulong? Size { get; set; }
        public required string Path { get; set; }

        public static FileInfoDto FromDirectoryInfo(DirectoryInfo directoryInfo, string memberDirectoryPath)
        {
            return new()
            {
                Name = directoryInfo.Name,
                FileType = FileType.Folder,
                Path = directoryInfo.FullName[memberDirectoryPath.Length..].TrimStart('/')
            };
        }

        public static FileInfoDto FromFileInfo(FileInfo fileInfo, string memberDirectoryPath)
            => new() {
                Name = fileInfo.Name,
                FileType = FileType.File,
                Extention = fileInfo.Extension,
                LastWriteTime = fileInfo.LastWriteTime.ToUniversalTime().Ticks,
                Size = (ulong?)fileInfo.Length,
                Path = fileInfo.FullName[memberDirectoryPath.Length..].TrimStart('/'),
            };
    }
}
