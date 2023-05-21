using Microsoft.Extensions.Caching.Memory;
using System.Net;
using VRGardenAlpha.Services.Security;

namespace VRGardenAlpha.Services.Analytics
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IMemoryCache _cache;
        private readonly IRemoteAddressService _remote;

        public AnalyticsService(IMemoryCache cache, IRemoteAddressService remote)
        {
            _cache = cache;
            _remote = remote;
        }
        
        public bool CanDownload(HttpContext ctx, int postId)
        {
            bool can = false;
            var ip = _remote.GetIPAddress(ctx);
            if (!_cache.TryGetValue(new Correlation(postId, ip, "download"), out DateTimeOffset ts))
                can = true;

            if ((DateTimeOffset.UtcNow - ts).TotalMinutes >= 60)
                can = true;

            if (can)
                _cache.Set(new Correlation(postId, ip, "download"), DateTimeOffset.UtcNow);

            return can;
        }

        public bool CanView(HttpContext ctx, int postId)
        {
            bool can = false;
            var ip = _remote.GetIPAddress(ctx);
            if (!_cache.TryGetValue(new Correlation(postId, ip, "view"), out DateTimeOffset ts))
                can = true;

            if ((DateTimeOffset.UtcNow - ts).TotalMinutes >= 30)
                can = true;

            if(can)
                _cache.Set(new Correlation(postId, ip, "view"), DateTimeOffset.UtcNow);
            
            return can;
        }

        public record Correlation(int PostId, IPAddress IP, string Context);
    }
}
