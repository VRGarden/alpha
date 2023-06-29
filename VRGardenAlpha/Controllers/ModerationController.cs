using AutoMapper;
using AutoMapper.QueryableExtensions;
using Meilisearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VRGardenAlpha.Data;
using VRGardenAlpha.Filters;
using VRGardenAlpha.Models;
using VRGardenAlpha.Models.Options;
using VRGardenAlpha.Services.Trading;

namespace VRGardenAlpha.Controllers
{
    [ApiController]
    [Route("/moderation")]
    public class ModerationController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IPackageInspectorService _packages;
        private readonly GardenContext _ctx;
        private readonly MeilisearchClient _client;
        private readonly GardenOptions _gardenOptions;

        public ModerationController(IPackageInspectorService packages, MeilisearchClient client, GardenContext ctx, IMapper mapper, IOptions<GardenOptions> gardenOptions)
        {
            _ctx = ctx;
            _client = client;
            _mapper = mapper;
            _packages = packages;
            _gardenOptions = gardenOptions.Value;
        }

        [HttpPost]
        [RequiresMaster]
        [Route("/_packages/delete-system")]
        public async Task<IActionResult> CleanPackagesAsync()
        {
            var packages = await _ctx.Posts.Where(x => x.FileName.EndsWith(".unitypackage"))
                .Where(x => x.Author == "System")
                .ToListAsync();

            foreach(var package in packages)
            {
                string path = Path.Combine("/var/www/vrcg-storage/" + package.Id + ".unitypackage");
                string imagePath = Path.Combine("/var/www/vrcg-storage/" + package.Id + "_image.jpg");
                System.IO.File.Delete(path);
                System.IO.File.Delete(imagePath);
                _ctx.Posts.Remove(package);
            }

            await _ctx.SaveChangesAsync();
            return Ok(true);
        }

        [HttpPost]
        [RequiresMaster]
        [Route("_search/settings/reindex")]
        public async Task<IActionResult> ReindexAsync([FromQuery] string? remoteHost = null)
        {
            var task = await _client.DeleteIndexAsync("vrcg-posts");
            while(true)
            {
                var current = await _client.GetTaskAsync(task.TaskUid);
                if (current.Status == TaskInfoStatus.Succeeded || current.Status == TaskInfoStatus.Failed)
                    break;
            }
            
            var index = _client.Index("vrcg-posts");
            await PushIndexSettingsAsync();

            var posts = await _ctx.Posts
                .Where(x => x.ACL == ACL.Public)
                .ToListAsync();

            var host = Request.Headers.Host;
            string proto = Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ??
                (Request.IsHttps ? "https" : "http");

            if (Request.Headers["X-Bot-Password"] == _gardenOptions.BotPassword)
                host = remoteHost ?? host;

            List<SearchablePost> sps = new();
            foreach(var post in posts)
            {
                var sp = _mapper.Map<SearchablePost>(post);
                sp.Thumbnail = proto + "://"
                    + host + "/@storage/"
                    + post.Id.ToString() + "_image"
                    + (post.ImageContentType == "image/gif" ? ".gif" : ".jpg");

                sps.Add(sp);
            }

            await index.AddDocumentsAsync(sps);
            return NoContent();
        }

        [HttpPost]
        [RequiresMaster]
        [Route("_search/settings/push")]
        public async Task<IActionResult> PushIndexSettingsAsync()
        {
            var index = _client.Index("vrcg-posts");

            await index.UpdateSearchableAttributesAsync(new string[]
            {
                "title",
                "description",
                "author",
                "creator"
            });

            await index.UpdateFilterableAttributesAsync(new string[]
            {
                "views",
                "downloads",
                "tags",
                "features",
                "platform",
                "contentLink"
            });

            await index.UpdateSortableAttributesAsync(new string[]
            {
                "title",
                "views",
                "downloads",
                "platform",
                "timestamp",
                "timestampISO"
            });

            return NoContent();
        }

        [HttpPost]
        [RequiresMaster]
        [Route("posts/{id}/acl")]
        public async Task<IActionResult> ModifyPostACL(int id, [FromBody] ModifyPostACLModel request)
        {
            var post = await _ctx.Posts.FindAsync(id);

            if (post == null)
                return NotFound();

            post.ACL = request.ACL;
            await _ctx.SaveChangesAsync();

            if(post.ACL != ACL.Public)
            {
                var index = _client.Index("vrcg-posts");
                await index.DeleteOneDocumentAsync(post.Id);
            }

            return NoContent();
        }
    }
}
