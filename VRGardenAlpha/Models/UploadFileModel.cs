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
        [Required]
        public required int Chunks { get; set; }

        [FromForm]
        [Required]
        public required int Chunk { get; set; }

        [FromForm]
        [Required]
        public required IFormFile Data { get; set; }
    }
}
