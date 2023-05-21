using System.Net;

namespace VRGardenAlpha.Data
{
    public class Post
    {
        public int Id { get; set; }                 // Identifier for the post
        public required string Title { get; set; }  // Title of the post
        public string? Description { get; set; }    // Description of the post
        public int Views { get; set; }              // Views on this post
        public int Downloads { get; set; }          // Downloads on this post
        public required ACL ACL { get; set; }       // Access Control List for this post
        public required Platform Platform { get; set; } // Platform of this post

        public string? Email { get; set; }              // Uploader's email (optional)
        public required string Author { get; set; }     // Uploader's username, that they specify
        public required IPAddress AuthorIP { get; set; } // Uploader's IP address
        // We only store this for legal protection, due to CSAM scanning done by Cloudflare
        // and potential reports that can be made, we may be inquired by legal entities to
        // hand over information related to a user; however we store no user data and anonymously upload content
        // so if any CSAM or abusive content is uploaded, we require this IP address to help protect the
        // integrity of the service in case any legal action is taken.
        
        public string? RemoteId { get; set; }               // Identifier if scraped from an external source (internal purposes)
        public string? ContentLink { get; set; }            // Link to other places the content may be found
        public required string Creator { get; set; }        // The name of the creator of the post, if differs from the author.

        public required string FileName { get; set; }       // Original name of the file
        public required string Checksum { get; set; }       // Checksum of the file (SHA-1 checksum)
        public required string ContentType { get; set; }    // Content type of the file, e.g. application/x-gzip
        public required long ContentLength { get; set; }    // Content length (size) of the file, e.g. 11765043
        
        public required string ImageContentType { get; set; }
        public long ImageContentLength { get; set; } = -1;

        public List<string> Tags { get; set; } = new List<string>();            // The tags describing the post
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;  // The exact time the post was created
        public int LastChunk { get; set; }
        public int Chunks { get; set; }
    }
}