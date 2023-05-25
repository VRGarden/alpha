using System.ComponentModel.DataAnnotations.Schema;

namespace VRGardenAlpha.Data
{
    public class Trade
    {
        public required string Id { get; set; } = Guid.NewGuid().ToString().Split('-')[0];
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public ACL ACL { get; set; } = ACL.Incomplete;
        
        [Column(TypeName = "jsonb")]
        public required TradeDetails Initiator { get; set; }
        
        [Column(TypeName = "jsonb")]
        public required TradeDetails Recepient { get; set; }

        public string[]? InitiatorPaths { get; set; }
        public string[]? RecepientPaths { get; set; }
    }

    public class TradeDetails
    {
        public required string Title { get; set; }
        public required string Creator { get; set; }
        public required string Trader { get; set; }
        public bool Agreed { get; set; } = false;
        public string? Passcode { get; set; }
        public string? ContentLink { get; set; }            // Link to other places the content may be found

        public required string FileName { get; set; }       // Original name of the file
        public required string Checksum { get; set; }       // Checksum of the file (SHA-1 checksum)
        public required string ContentType { get; set; }    // Content type of the file, e.g. application/x-gzip
        public long ContentLength { get; set; } = -1;       // Content length (size) of the file, e.g. 11765043

        public required string ImageContentType { get; set; }
        public long ImageContentLength { get; set; } = -1;

        public int LastChunk { get; set; }
        public int Chunks { get; set; }
    }
}
