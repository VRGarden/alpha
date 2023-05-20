using AutoMapper;
using Meilisearch;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using VRGardenAlpha.Data;
using VRGardenAlpha.Filters;
using VRGardenAlpha.Models;
using VRGardenAlpha.Models.Options;
using VRGardenAlpha.Services.Security;

namespace VRGardenAlpha.Controllers
{
    [ApiController]
    [Route("/posts")]
    public class CreatePostController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly GardenContext _ctx;
        private readonly StorageOptions _options;
        private readonly MeilisearchClient _client;
        private readonly GardenOptions _gardenOptions;
        private readonly IRemoteAddressService _remote;
        private readonly ILogger<CreatePostController> _logger;

        private readonly static string[] _allowedImageTypes = new string[]
        {
            "image/png",
            "image/webp",
            "image/jpeg",
            "image/jpg",
            "image/gif"
        };

        public CreatePostController(
                IMapper mapper,
                GardenContext ctx,
                MeilisearchClient client,
                IRemoteAddressService remote,
                IOptions<StorageOptions> options,
                ILogger<CreatePostController> logger,
                IOptions<GardenOptions> gardenOptions
            )
        {
            _ctx = ctx;
            _client = client;
            _mapper = mapper;
            _logger = logger;
            _remote = remote;
            _options = options.Value;
            _gardenOptions = gardenOptions.Value;
        }

        [HttpPost]
        [RequiresCaptcha]
        [ProducesResponseType(200, Type = typeof(PostModel))]
        public async Task<IActionResult> CreatePostAsync([FromBody] CreatePostModel request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (request.RemoteId != null || request.Author == "System")
            {
                string? header = Request.Headers["X-Bot-Password"];
                if (header != _gardenOptions.BotPassword)
                    return Unauthorized(new { error = "credentials.invalid" });
            }

            var post = new Post()
            {
                ACL = ACL.Incomplete,
                Title = request.Title,
                Description = request.Description,
                Email = request.Email,
                Author = request.Author,
                ContentLength = -1,
                Creator = request.Creator,
                RemoteId = request.RemoteId,
                ContentLink = request.ContentLink,
                ImageContentType = "image/jpeg",
                Checksum = "PENDING_FILE_UPLOAD",
                FileName = "PENDING_FILE_UPLOAD",
                ContentType = "application/octet-stream",
                AuthorIP = _remote.GetIPAddress(HttpContext)
            };

            await _ctx.Posts.AddAsync(post);
            await _ctx.SaveChangesAsync();

            return Ok(_mapper.Map<PostModel>(post));
        }

        [HttpPost("{id}/image")]
        public async Task<IActionResult> UploadImageAsync(int id, [FromForm][Required(ErrorMessage = "image.required")] IFormFile image)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            double length = (image.Length) / (1024.0 * 1024.0);
            if (length > 5.0) // Max 5MB image limit
                return BadRequest(new { error = "image.size" });

            var post = await _ctx.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            if (post.ACL != ACL.Incomplete)
                return BadRequest(new { error = "post.complete" });

            if (post.ImageContentLength > 0)
                return BadRequest(new { error = "image.complete" });

            if (!_allowedImageTypes.Contains(image.ContentType))
                return BadRequest(new { error = "contentType.invalid" });

            using var @is = await Image.LoadAsync(image.OpenReadStream());
            string ext = image.ContentType == "image/gif" ? ".gif" : ".jpg";
            string path = Path.Combine(_options.MountPath!, post.Id.ToString() + "_image" + ext);

            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                if (image.ContentType == "image/gif")
                    await @is.SaveAsGifAsync(fs);
                else
                    await @is.SaveAsJpegAsync(fs);
            }

            var fi = new FileInfo(path);
            post.ImageContentType = image.ContentType;
            post.ImageContentLength = fi.Length;

            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/file")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> UploadFileAsync(int id, [FromForm] UploadFileModel request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var post = await _ctx.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            if (post.ACL != ACL.Incomplete)
                return BadRequest(new { error = "post.complete" });

            if (post.ImageContentLength < 0)
                return BadRequest(new { error = "image.required" });

            if (request.Chunk > (post.LastChunk + 1) || request.Chunk <= post.LastChunk)
                return BadRequest(new { error = "chunkposition.invalid" });

            if (request.ContentType != "application/gzip" && request.ContentType != "application/zip")
                return BadRequest(new { error = "contentType.invalid" });

            if (request.Chunk == 1 && post.LastChunk < 1)
            {
                post.Chunks = request.Chunks;
                post.ContentType = request.ContentType;
                post.FileName = WebUtility.HtmlEncode(request.FileName);
            }
            else if (post.LastChunk < 1)
                return BadRequest(new { error = "upload.failure" });

            if (request.Chunks != post.Chunks)
                return BadRequest(new { error = "file.changed" });

            if (request.Chunk > post.Chunks)
                return BadRequest(new { error = "chunks.max" });

            double length = (request.Data.Length + post.ContentLength) / (1024.0 * 1024.0 * 1024.0);
            if (length > 3.2) // 3GB max limit.
                return BadRequest(new { error = "file.size" });

            string ext = post.ContentType == "application/gzip" ? ".unitypackage" : ".zip";
            string path = Path.Combine(_options.MountPath!, post.Id.ToString() + ext);
            using (var fs = new FileStream(path, FileMode.Append))
            using (var data = request.Data.OpenReadStream())
            {
                await data.CopyToAsync(fs);
                fs.Close();

                post.LastChunk = request.Chunk;
                post.ContentLength += data.Length;
            }

            if (post.Chunks == post.LastChunk)
            {
                if (!VerifyFileType(post, path))
                    return BadRequest(new { error = "validation.failure" });

                var hash = await SHA1.HashDataAsync(new FileStream(path, FileMode.Open));
                post.Checksum = Convert.ToHexString(hash);
                post.ACL = ACL.Public;

                string proto = Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? 
                    (Request.IsHttps ? "https" : "http");

                var index = _client.Index("vrcg-posts");
                var sp = _mapper.Map<SearchablePost>(post);
                sp.Thumbnail = proto + "://"
                    + Request.Headers.Host + "/@storage/"
                    + post.Id.ToString() + "_image"
                    + (post.ImageContentType == "image/gif" ? ".gif" : ".jpg");

                await index.AddDocumentsAsync(new SearchablePost[] { sp });
            }

            await _ctx.SaveChangesAsync();

            return Ok(new
            {
                length = post.ContentLength,
                chunk = post.LastChunk,
                remaining = post.Chunks - request.Chunk,
                completed = post.Chunks == post.LastChunk
            });
        }

        [NonAction]
        private bool VerifyFileType(Post post, string path)
        {
            if (post.ContentType == "application/gzip")
                return VerifyUnityPackageBase(path);
            else
                return VerifyZip(path);
        }

        [NonAction]
        private bool VerifyUnityPackageBase(string path)
        {
            using var fs = new FileStream(path, FileMode.Open);
            var first = fs.ReadByte();
            var second = fs.ReadByte();

            return first == 0x1F && second == 0x8B;
        }

        [NonAction]
        private bool VerifyZip(string path)
        {
            try
            {
                ZipArchive archive = ZipFile.OpenRead(path);
                archive.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}