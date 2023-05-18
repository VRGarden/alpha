using AutoMapper;
using Meilisearch;
using Microsoft.AspNetCore.Mvc;
using VRGardenAlpha.Data;
using VRGardenAlpha.Models;
using VRGardenAlpha.Services.Analytics;

namespace VRGardenAlpha.Controllers
{
    [ApiController]
    [Route("/browse")]
    public class BrowseController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly GardenContext _ctx;
        private readonly MeilisearchClient _client;
        private readonly IAnalyticsService _analytics;

        public BrowseController(GardenContext ctx, IAnalyticsService analytics, MeilisearchClient client, IMapper mapper)
        {
            _ctx = ctx;
            _mapper = mapper;
            _client = client;
            _analytics = analytics;
        }

        [HttpGet("posts/{id}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(PostModel))]
        public async Task<IActionResult> GetPostAsync(int id)
        {
            var post = await _ctx.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            return Ok(_mapper.Map<PostModel>(post));
        }

        [ProducesResponseType(204)]
        [HttpGet("posts/{id}/analytics")]
        public async Task<IActionResult> ProcessPostAnalyticsAsync(int id, [FromQuery] string action)
        {
            var post = await _ctx.Posts.FindAsync(id);
            if (post == null)
                return NoContent(); // Return from processing if the post doesn't exist.

            var index = _client.Index("vrcg-posts");
            if (action == "view" && _analytics.CanView(HttpContext, id))
                post.Views++;
            else if (action == "download" && _analytics.CanDownload(HttpContext, id))
                post.Downloads++;
            else return NoContent(); // This is here to prevent any transactions from being processed.

            await _ctx.SaveChangesAsync();
            await index.UpdateDocumentsAsync(new SearchablePost[] { _mapper.Map<SearchablePost>(post) });
            // fuck you meilisearch for not implementing single document operations

            return NoContent();
        }
    }
}
