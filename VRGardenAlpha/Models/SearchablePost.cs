using VRGardenAlpha.Data;

namespace VRGardenAlpha.Models
{
    public class SearchablePost
    {
        public int Id { get; set; }                 // Identifier for the post
        public required string Title { get; set; }  // Title of the post
        public string? Description { get; set; }    // Description of the post
        public int Views { get; set; }              // Views on this post
        public int Downloads { get; set; }          // Downloads on this post
        public required Platform Platform { get; set; } // Platform for this post

        public required string Author { get; set; }     // Uploader's username, that they specify
        public required string Creator { get; set; }    // The name of the creator of the post, if differs from the author.
        public string? ContentLink { get; set; }        // Link to other places the content may be found

        public string? Thumbnail { get; set; }
        public required string FileName { get; set; }                           // Original name of the file
        public List<string> Tags { get; set; } = new List<string>();            // The tags describing the post
        public List<string> Features { get; set; } = new List<string>();        // The special tags describing the post
        public DateTimeOffset TimestampISO { get; set; } = DateTimeOffset.UtcNow;  // The exact time the post was created
        public long Timestamp => TimestampISO.ToUnixTimeSeconds();  // The exact time the post was created in Unix Time
    }
}
