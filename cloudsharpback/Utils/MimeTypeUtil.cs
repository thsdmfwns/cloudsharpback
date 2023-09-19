using Microsoft.AspNetCore.StaticFiles;

namespace cloudsharpback.Utils
{
    public static class MimeTypeUtil
    {
        private static readonly List<string> viewableExtensions = new()
        {
            ".gif", ".jpg", ".jpeg", ".png", ".svg", ".webp", ".bmp", ".ico", ".webm", ".ogg", ".mp4", ".pdf", ".zip", ".docx"
        };

        public static bool CanViewInFront(string extension)
            => viewableExtensions.Contains(extension); 

        public static string? GetExtension(string contentType)
            => new FileExtensionContentTypeProvider().Mappings.FirstOrDefault(x => x.Key == contentType).Value;

        public static string? GetMimeType(string filepath)
            => new FileExtensionContentTypeProvider().TryGetContentType(filepath, out var mime) ? mime : null;
    }
}
