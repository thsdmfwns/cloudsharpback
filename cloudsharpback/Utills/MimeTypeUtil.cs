using Microsoft.AspNetCore.StaticFiles;

namespace cloudsharpback.Utills
{
    public static class MimeTypeUtil
    {
        public static string? GetExtension(string contentType)
            => new FileExtensionContentTypeProvider().Mappings.FirstOrDefault(x => x.Key == contentType).Value;
        public static string? GetMimeType(string filepath)
        {
            if (!new FileExtensionContentTypeProvider().TryGetContentType(filepath, out var mime))
            {
                return null;
            }
            return mime;
        }
    }
}
