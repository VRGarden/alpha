using System.ComponentModel.DataAnnotations;
using VRGardenAlpha.Data;

namespace VRGardenAlpha.Models
{
    public class CreatePostModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "title.required")]
        [StringLength(64, MinimumLength = 3, ErrorMessage = "title.length")]
        public required string Title { get; set; }  // Title of the post

        [MaxLength(10_000, ErrorMessage = "description.length")]
        public string? Description { get; set; }    // Description of the post

        [EmailAddress(ErrorMessage = "email.invalid")]
        public string? Email { get; set; }        // Uploader's email (optional)

        [Required(AllowEmptyStrings = false, ErrorMessage = "author.required")]
        [StringLength(32, MinimumLength = 2, ErrorMessage = "author.length")]
        public required string Author { get; set; } // Uploader's username, that they specify

        [MaxLength(100, ErrorMessage = "remoteId.length")]
        public string? RemoteId { get; set; }               // Identifier if scraped from an external source (internal purposes)

        [Url(ErrorMessage = "contentLink.invalid")]
        [MaxLength(365, ErrorMessage = "contentLink.length")]
        public string? ContentLink { get; set; }            // Link to other places the content may be found

        [Required(AllowEmptyStrings = false, ErrorMessage = "creator.required")]
        [StringLength(64, MinimumLength = 2, ErrorMessage = "creator.length")]
        public required string Creator { get; set; }    // The name of the creator of the post, if differs from the author.

        [Required(ErrorMessage = "platform.required")]
        public required Platform Platform { get; set; }

        [Required(ErrorMessage = "tags.required")]
        public List<string> Tags { get; set; } = new List<string>();    // The tags describing the post

        [Required(ErrorMessage = "features.required")]
        public List<string> Features { get; set; } = new List<string>(); // The special tags describing the post
    }
}
