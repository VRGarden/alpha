using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VRGardenAlpha.Models.Options;

namespace VRGardenAlpha.Controllers
{
    [ApiController]
    [Route("/settings/spa")]
    public class SpaController : ControllerBase
    {
        private readonly GardenOptions _gardenOptions;
        private readonly SearchOptions _searchOptions;
        private readonly CaptchaOptions _captchaOptions;
        
        public SpaController(
                IOptions<GardenOptions> gardenOptions,
                IOptions<SearchOptions> searchOptions,
                IOptions<CaptchaOptions> captchaOptions
            )
        {
            _gardenOptions = gardenOptions.Value;
            _searchOptions = searchOptions.Value;
            _captchaOptions = captchaOptions.Value;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        public Task<OkObjectResult> GetSpaSettingsAsync()
        {
            return Task.FromResult(Ok(new
            {
                garden = new
                {
                    captcha = _gardenOptions.RequiresCaptcha,
                    discord = _gardenOptions.Discord,
                    key = _captchaOptions.SiteKey
                },
                search = new
                {
                    endpoint = _searchOptions.Endpoint,
                    key = _searchOptions.SearchKey
                }
            }));
        }
    }
}
