using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace VRGardenAlpha.Models
{
    public class TradeImageUploadModel
    {
        [FromForm]
        [Required(AllowEmptyStrings = false, ErrorMessage = "initiatorImage.required")]
        public required IFormFile InitiatorImage { get; set; }
        
        [FromForm]
        [Required(AllowEmptyStrings = false, ErrorMessage = "recipientImage.required")]
        public required IFormFile RecipientImage { get; set; }
    }
}
