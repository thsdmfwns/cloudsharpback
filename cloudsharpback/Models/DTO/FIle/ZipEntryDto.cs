using System.IO.Compression;

namespace cloudsharpback.Models.DTO.FIle
{
    public class ZipEntryDto
    {
        public ZipEntryDto(string name, long length, long compressLength, string extension, long lastWriteTime, string path, bool isFolder)
        {
            Name = name;
            Length = length;
            CompressLength = compressLength;
            Extension = extension;
            LastWriteTime = lastWriteTime;
            FilePath = path;
            IsFolder = isFolder;
        }

        public string Name { get; set; }
        public long Length { get; set; }
        public long CompressLength { get; set; }
        public string Extension { get; set; }
        public string FilePath { get; set; }
        public bool IsFolder { get; set; }
        public long LastWriteTime { get; set; }

        public static ZipEntryDto FromEntry(ZipArchiveEntry entry)
        {
            int nameLength = entry.Name.Length;
            var isfol = ((nameLength > 0) &&
                ((entry.Name[nameLength - 1] == '/') || (entry.Name[nameLength - 1] == '\\'))) ||
                ((entry.ExternalAttributes & 16) != 0);
            var extension = Path.GetExtension(entry.FullName);
            return new ZipEntryDto(
                name: entry.Name,
                length: entry.Length,
                compressLength: entry.CompressedLength,
                extension: extension,
                lastWriteTime: entry.LastWriteTime.Ticks,
                path: entry.FullName,
                isFolder: isfol
            );
        }
    }
}
