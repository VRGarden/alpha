using Microsoft.Extensions.Options;
using VRGardenAlpha.Models.Options;

namespace VRGardenAlpha.Services.Security
{
    public class CloudflareCaptchaService : ICaptchaService
    {
        private readonly HttpClient _client;
        private readonly CaptchaOptions _options;
        private readonly IWebHostEnvironment _env;
        private readonly IRemoteAddressService _remote;

        public CloudflareCaptchaService(IOptions<CaptchaOptions> options, IRemoteAddressService remote, IHttpClientFactory factory, IWebHostEnvironment env)
        {
            _env = env;
            _remote = remote;
            _options = options.Value;
            _client = factory.CreateClient();
        }

        public async Task<bool> VerifyCaptchaAsync(string code, HttpContext? ctx = null)
        {
            if (_env.IsDevelopment())
                return true;

            string? remoteip = null;
            if (ctx != null)
                remoteip = _remote.GetIPAddress(ctx).ToString();
            
            var data = new
            {
                secret = _options.SecretKey,
                response = code,
                remoteip,
            };

            using var response = await _client.PostAsJsonAsync(_options.Endpoint, data);
            var captcha = await response.Content.ReadFromJsonAsync<CaptchaResponse>();

            return captcha?.Success == true;
        }
    }

    public class CaptchaResponse
    {
        public bool Success { get; set; }
    }
}