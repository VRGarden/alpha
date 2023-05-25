using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace VRGardenAlpha.Models
{
    public class UploadFileModel
    {
        [FromForm]
        [Required(AllowEmptyStrings = false, ErrorMessage = "fileName.required")]
        public required string FileName { get; set; }

        [FromForm]
        [Required(AllowEmptyStrings = false, ErrorMessage = "contentType.required")]
        public required string ContentType { get; set; }

        [FromForm]
        public string? RemoteHost { get; set; }

        [FromForm]
        public string? Passcode { get; set; }

        [FromForm]
        public string? Role { get; set; }

        [FromForm]
        [Required(AllowEmptyStrings = false, ErrorMessage = "chunks.required")]
        public required int Chunks { get; set; }

        [FromForm]
        [Required(AllowEmptyStrings = false, ErrorMessage = "chunk.required")]
        public required int Chunk { get; set; }

        [FromForm]
        [Required(AllowEmptyStrings = false, ErrorMessage = "data.required")]
        public required IFormFile Data { get; set; }
    }
}
