using AngleSharp;
using Microsoft.AspNetCore.Mvc;
using VRGardenAlpha.Models;

namespace VRGardenAlpha.Controllers
{
    [ApiController]
    [Route("/trades/information")]
    public class TradeInfoController : ControllerBase
    {
        private readonly HttpClient _client;
        private readonly IBrowsingContext _context;

        public TradeInfoController(IHttpClientFactory factory)
        {
            _client = factory.CreateClient();
            _context = BrowsingContext.New();
        }

        [HttpGet]
        [Route("booth")]
        public async Task<IActionResult> GetBoothInformationAsync([FromQuery] Uri url)
        {
            if (!url.Host.Contains("booth.pm"))
                return BadRequest(new { error = "url.invalid" });

            try
            {
                string id = url.AbsolutePath.Split('/').Last();
                var data = await _client.GetFromJsonAsync<BoothResult>($"https://booth.pm/en/items/{id}.json");

                return Ok(new
                {
                    title = data?.Name,
                    creator = data?.Shop?.Name,
                    image = data?.Images.FirstOrDefault()?.Original
                });
            }
            catch
            {
                Response.StatusCode = 500;
                return new JsonResult(new { error = "information.failure" });
            }
        }

        [HttpGet]
        [Route("payhip")]
        public async Task<IActionResult> GetPayHipInformationAsync([FromQuery] Uri url)
        {
            if(!url.Host.Contains("payhip.com"))
                return BadRequest(new { error = "url.invalid" });

            try
            {
                var html = await _client.GetStringAsync(url);
                using var document = await _context.OpenAsync(req => req.Content(html));

                var title = document.QuerySelector("meta[property='og:title']")?.GetAttribute("content");
                var image = document.QuerySelector("meta[property='og:image']")?.GetAttribute("content");
                var creator = document.QuerySelector("meta[property='og:site_name']")?.GetAttribute("content");

                return Ok(new
                {
                    title,
                    creator,
                    image,
                });
            }
            catch
            {
                Response.StatusCode = 500;
                return new JsonResult(new { error = "information.failure" });
            }
        }

        [HttpGet]
        [Route("gumroad")]
        public async Task<IActionResult> GetGumroadInformationAsync([FromQuery] Uri url)
        {
            if (!url.Host.Contains("gumroad.com"))
                return BadRequest(new { error = "url.invalid" });
            
            try
            {
                var html = await _client.GetStringAsync(url);
                using var document = await _context.OpenAsync(req => req.Content(html));

                var image = document.QuerySelector("link[as='image']")?.GetAttribute("href");
                var title = document.QuerySelector("meta[property='og:title']")?.GetAttribute("content");
                var creator = document.QuerySelector("a[class='user']")?.TextContent?.Trim();

                return Ok(new
                {
                    title,
                    creator,
                    image,
                });
            }
            catch
            {
                Response.StatusCode = 500;
                return new JsonResult(new { error = "information.failure" });
            }
        }
    }
}
