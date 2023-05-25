using System.ComponentModel.DataAnnotations;

namespace VRGardenAlpha.Models
{
    public class CreateTradeModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "title.required")]
        [StringLength(64, MinimumLength = 3, ErrorMessage = "title.length")]
        public required string Title { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "creator.required")]
        [StringLength(64, MinimumLength = 2, ErrorMessage = "creator.length")]
        public required string Creator { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "trader.required")]
        [StringLength(32, MinimumLength = 2, ErrorMessage = "trader.length")]
        public required string Trader { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "passcode.required")]
        [StringLength(1024, MinimumLength = 4, ErrorMessage = "passcode.length")]
        public required string Passcode { get; set; }

        [Url(ErrorMessage = "contentLink.invalid")]
        [MaxLength(512, ErrorMessage = "contentLink.length")]
        public string? ContentLink { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "recipient.required")]
        public required CreateTradeModel Recipient { get; set; }
    }
}
