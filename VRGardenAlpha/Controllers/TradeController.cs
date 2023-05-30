using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using VRGardenAlpha.Data;
using VRGardenAlpha.Filters;
using VRGardenAlpha.Models;
using VRGardenAlpha.Models.Options;
using VRGardenAlpha.Services.Trading;

namespace VRGardenAlpha.Controllers
{
    [ApiController]
    [Route("/trades")]
    public class TradeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly GardenContext _ctx;
        private readonly StorageOptions _options;
        private readonly IPackageInspectorService _packageInspector;
        
        private readonly static string[] _allowedImageTypes = new string[]
        {
            "image/png",
            "image/webp",
            "image/jpeg",
            "image/jpg",
            "image/gif"
        };
        
        public TradeController(GardenContext ctx, IMapper mapper, IOptions<StorageOptions> options, IPackageInspectorService packageInspector)
        {
            _ctx = ctx;
            _mapper = mapper;
            _options = options.Value;
            _packageInspector = packageInspector;
        }

        [HttpGet]
        [Route("stats")]
        public async Task<IActionResult> GetStatsAsync()
        {
            var count = await _ctx.Trades.CountAsync();
            var active = await _ctx.Trades.CountAsync(x => x.Initiator.Accepted && x.Recipient.Accepted);

            return Ok(new
            {
                count,
                active
            });
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetTradeAsync(string id)
        {
            var trade = await _ctx.Trades
                .ProjectTo<TradeModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (trade == null)
                return NotFound();
            
            if (trade.ACL != ACL.Public)
                return NotFound();
            
            return Ok(trade);
        }

        [HttpGet]
        [Route("{id}/download")]
        public async Task<IActionResult> DownloadTradedAvatarAsync(string id, [FromQuery] string passcode)
        {
            var trade = await _ctx.Trades.FindAsync(id);
            if (trade == null)
                return NotFound();

            if (trade.ACL != ACL.Public)
                return NotFound();

            string role;
            if (trade.Initiator.Passcode == passcode)
                role = "initiator";
            else if (trade.Recipient.Passcode == passcode)
                role = "recipient";
            else
                return Unauthorized(new { error = "credentials.invalid" });

            string path = Path.Combine(_options.TradeMountPath!, trade.Id + "_" + role + ".unitypackage");
            return PhysicalFile(Path.GetFullPath(path), "application/gzip", true);
        }

        [HttpPost]
        [Route("{id}/accept")]
        public async Task<IActionResult> TradeAcceptAsync(string id, [FromQuery] string passcode)
        {
            var trade = await _ctx.Trades.FindAsync(id);
            if (trade == null)
                return NotFound();

            if (trade.ACL != ACL.Public)
                return NotFound();

            if (trade.Initiator.Passcode == passcode)
                trade.Initiator.Accepted = true;

            if (trade.Recipient.Passcode == passcode)
                trade.Recipient.Accepted = true;

            bool completed = false;
            if (trade.Recipient.Accepted && trade.Initiator.Accepted)
                completed = true;

            await _ctx.SaveChangesAsync();
            return Ok(new { accepted = true, completed });
        }

        [HttpPost]
        [RequiresCaptcha]
        [ProducesResponseType(200, Type = typeof(TradeModel))]
        public async Task<IActionResult> CreateTradeAsync([FromBody] CreateTradeModel request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var trade = new Trade()
            {
                ACL = ACL.Incomplete,
                Initiator = new()
                {
                    Title = request.Title,
                    Trader = request.Trader,
                    Creator = request.Creator,
                    Passcode = request.Passcode,
                    ContentLink = request.ContentLink,
                    ContentLength = -1,
                    ImageContentType = "image/jpeg",
                    ContentType = "application/octet-stream",
                    Checksum = "PENDING_FILE_UPLOAD",
                    FileName = "PENDING_FILE_UPLOAD",
                },
                Recipient = new()
                {
                    Title = request.Recipient.Title,
                    Trader = request.Recipient.Trader,
                    Creator = request.Recipient.Creator,
                    ContentLink = request.Recipient.ContentLink,
                    ContentLength = -1,
                    ImageContentType = "image/jpeg",
                    ContentType = "application/octet-stream",
                    Checksum = "PENDING_FILE_UPLOAD",
                    FileName = "PENDING_FILE_UPLOAD",
                }
            };

            await _ctx.Trades.AddAsync(trade);
            await _ctx.SaveChangesAsync();

            return Ok(_mapper.Map<TradeModel>(trade));
        }

        [HttpPost("{id}/images")]
        public async Task<IActionResult> UploadTradeImagesAsync(string id, [FromForm] TradeImageUploadModel request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            double length = (request.InitiatorImage.Length) / (1024.0 * 1024.0);
            if (length > 5.0) // Max 5MB image limit
                return BadRequest(new { error = "initiatorImage.size" });

            length = (request.RecipientImage.Length) / (1024.0 * 1024.0);
            if (length > 5.0) // Max 5MB image limit
                return BadRequest(new { error = "recipientImage.size" });

            var trade = await _ctx.Trades.FindAsync(id);
            if (trade == null)
                return NotFound();

            if (trade.ACL != ACL.Incomplete)
                return BadRequest(new { error = "files.complete" });

            if (trade.Initiator.ImageContentLength > 0)
                return BadRequest(new { error = "images.complete" });

            if (!_allowedImageTypes.Contains(request.InitiatorImage.ContentType))
                return BadRequest(new { error = "initiatorImage.contentType.invalid" });

            if (!_allowedImageTypes.Contains(request.RecipientImage.ContentType))
                return BadRequest(new { error = "recipientImage.contentType.invalid" });

            // Initiator Image
            string ext = request.InitiatorImage.ContentType == "image/gif" ? ".gif" : ".jpg";
            string path = Path.Combine(_options.MountPath!, "trade_" + trade.Id + "_initiator_image" + ext);
            await ImageProcessor.ProcessImage(request.InitiatorImage.OpenReadStream(), request.InitiatorImage.ContentType, path);

            var fi = new FileInfo(path);
            trade.Initiator.ImageContentType = request.InitiatorImage.ContentType == "image/gif" 
                ? request.InitiatorImage.ContentType : "image/jpeg";
            trade.Initiator.ImageContentLength = fi.Length;

            // Recipient Image
            ext = request.RecipientImage.ContentType == "image/gif" ? ".gif" : ".jpg";
            path = Path.Combine(_options.MountPath!, "trade_" + trade.Id + "_recipient_image" + ext);
            await ImageProcessor.ProcessImage(request.RecipientImage.OpenReadStream(), request.RecipientImage.ContentType, path);

            fi = new FileInfo(path);
            trade.Recipient.ImageContentType = request.RecipientImage.ContentType == "image/gif"
                ? request.RecipientImage.ContentType : "image/jpeg";
            trade.Recipient.ImageContentLength = fi.Length;

            _ctx.Entry(trade).State = EntityState.Modified;
            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/file")]
        public async Task<IActionResult> UploadTradeFileAsync(string id, [FromForm] UploadFileModel request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var trade = await _ctx.Trades.FindAsync(id);
            if (trade == null)
                return NotFound();

            if (trade.Initiator.ImageContentLength < 0)
                return BadRequest(new { error = "image.required" });

            var curr = request.Role == "initiator" ? trade.Initiator : 
                request.Role == "recipient" ? trade.Recipient : throw new ArgumentException("Invalid role");

            if (request.Passcode != curr.Passcode)
                return Unauthorized(new { error = "credentials.invalid" });

            if (curr.LastChunk == curr.Chunks)
                return BadRequest(new { error = "file.complete" });

            if (request.Chunk > (curr.LastChunk + 1) || request.Chunk <= curr.LastChunk)
                return BadRequest(new { error = "chunkposition.invalid" });

            if (request.ContentType != "application/gzip")
                return BadRequest(new { error = "contentType.invalid" });

            if (request.Chunk == 1 && curr.LastChunk < 1)
            {
                curr.Chunks = request.Chunks;
                curr.ContentType = request.ContentType;
                curr.FileName = WebUtility.HtmlEncode(request.FileName);
            }
            else if (curr.LastChunk < 1)
                return BadRequest(new { error = "upload.failure" });

            if (request.Chunks != curr.Chunks)
                return BadRequest(new { error = "file.changed" });

            if (request.Chunk > curr.Chunks)
                return BadRequest(new { error = "chunks.max" });

            double length = (request.Data.Length + curr.ContentLength) / (1024.0 * 1024.0 * 1024.0);
            if (length > 4.2) // 4GB max limit.
                return BadRequest(new { error = "file.size" });

            string path = Path.Combine(_options.TradeMountPath!, trade.Id + "_" + request.Role + ".unitypackage");
            using (var fs = new FileStream(path, FileMode.Append))
            using (var data = request.Data.OpenReadStream())
            {
                await data.CopyToAsync(fs);
                fs.Close();

                curr.LastChunk = request.Chunk;
                curr.ContentLength += request.Data.Length;
            }

            if (curr.Chunks == curr.LastChunk)
            {
                var hash = await SHA1.HashDataAsync(new FileStream(path, FileMode.Open));
                curr.Checksum = Convert.ToHexString(hash);

                var paths = await _packageInspector.GetPackagePathsAsync(path);
                if (request.Role == "initiator")
                {
                    trade.ACL = ACL.Public;
                    trade.InitiatorPaths = paths;
                }
                else trade.RecipientPaths = paths;
            }

            _ctx.Entry(trade).State = EntityState.Modified;
            await _ctx.SaveChangesAsync();

            return Ok(new
            {
                length = curr.ContentLength,
                chunk = curr.LastChunk,
                remaining = curr.Chunks - request.Chunk,
                completed = curr.Chunks == curr.LastChunk
            });
        }
    }
}
