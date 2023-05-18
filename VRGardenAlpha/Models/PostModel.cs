using VRGardenAlpha.Data;

namespace VRGardenAlpha.Models
{
    public class PostModel
    {
        public int Id { get; set; }                 // Identifier for the post
        public required string Title { get; set; }  // Title of the post
        public string? Description { get; set; }    // Description of the post
        public int Views { get; set; }              // Views on this post
        public int Downloads { get; set; }          // Downloads on this post
        public required ACL ACL { get; set; }       // Access Control List for this post
        
        public required string Author { get; set; }     // Uploader's username, that they specify
        public required string Creator { get; set; }    // The name of the creator of the post, if differs from the author.
        public string? ContentLink { get; set; }        // Link to other places the content may be found

        public required string FileName { get; set; }       // Original name of the file
        public required string Checksum { get; set; }       // Checksum of the file (SHA-1 checksum)
        public required string ContentType { get; set; }    // Content type of the file, e.g. application/x-gzip
        public required long ContentLength { get; set; }   // Content length (size) of the file, e.g. 11765043

        public required string ImageContentType { get; set; }
        public long ImageContentLength { get; set; }
        
        public List<string> Tags { get; set; } = new List<string>();            // The tags describing the post
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;  // The exact time the post was created
    }
}
