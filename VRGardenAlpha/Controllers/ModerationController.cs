using Meilisearch;
using Microsoft.AspNetCore.Mvc;
using VRGardenAlpha.Data;
using VRGardenAlpha.Filters;
using VRGardenAlpha.Models;

namespace VRGardenAlpha.Controllers
{
    [ApiController]
    [Route("/moderation")]
    public class ModerationController : ControllerBase
    {
        private readonly GardenContext _ctx;
        private readonly MeilisearchClient _client;

        public ModerationController(MeilisearchClient client, GardenContext ctx)
        {
            _ctx = ctx;
            _client = client;
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
                "timestamp"
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
