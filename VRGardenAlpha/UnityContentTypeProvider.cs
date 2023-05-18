using Microsoft.AspNetCore.StaticFiles;
using System.Diagnostics.CodeAnalysis;

namespace VRGardenAlpha
{
    public class UnityContentTypeProvider : IContentTypeProvider
    {
        public bool TryGetContentType(string subpath, [MaybeNullWhen(false)] out string contentType)
        {
            var ext = Path.GetExtension(subpath);

            contentType = ext switch
            {
                ".gif" => "image/gif",
                ".jpg" => "image/jpeg",
                ".unitypackage" => "application/gzip",
                ".zip" => "application/zip",
                _ => null
            };

            return contentType != null;
        }
    }
}
