using VRGardenAlpha.Data;

namespace VRGardenAlpha.Models
{
    public class TradeModel
    {
        public required string Id { get; set; }
        public required ACL ACL { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        public required TradeDetailsModel Initiator { get; set; }
        public required TradeDetailsModel Recipient { get; set; }

        public string[]? InitiatorPaths { get; set; }
        public string[]? RecipientPaths { get; set; }
    }

    public class TradeDetailsModel
    {
        public required string Title { get; set; }
        public required string Creator { get; set; }
        public required string Trader { get; set; }
        public bool Agreed { get; set; }
        public string? ContentLink { get; set; }            // Link to other places the content may be found

        public required string FileName { get; set; }       // Original name of the file
        public required string Checksum { get; set; }       // Checksum of the file (SHA-1 checksum)
        public required string ContentType { get; set; }    // Content type of the file, e.g. application/x-gzip
        public long ContentLength { get; set; } = -1;       // Content length (size) of the file, e.g. 11765043

        public required string ImageContentType { get; set; }
        public long ImageContentLength { get; set; } = -1;
    }
}
