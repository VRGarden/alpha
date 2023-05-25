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

namespace VRGardenAlpha.Controllers
{
    [ApiController]
    [Route("/moderation")]
    public class ModerationController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly GardenContext _ctx;
        private readonly MeilisearchClient _client;
        private readonly GardenOptions _gardenOptions;

        public ModerationController(MeilisearchClient client, GardenContext ctx, IMapper mapper, IOptions<GardenOptions> gardenOptions)
        {
            _ctx = ctx;
            _client = client;
            _mapper = mapper;
            _gardenOptions = gardenOptions.Value;
        }

        [HttpPost]
        [RequiresMaster]
        [Route("_search/settings/reindex")]
        public async Task<IActionResult> ReindexAsync([FromQuery] string? remoteHost = null)
        {
            var index = _client.Index("vrcg-posts");
            await index.DeleteAsync();

            var posts = await _ctx.Posts
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

            return NoContent();
        }
    }
}
