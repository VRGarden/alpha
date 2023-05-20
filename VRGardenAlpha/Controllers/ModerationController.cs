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

        public ModerationController(GardenContext ctx)
        {
            _ctx = ctx;
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
