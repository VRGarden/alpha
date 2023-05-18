namespace VRGardenAlpha.Services.Analytics
{
    public interface IAnalyticsService
    {
        bool CanView(HttpContext ctx, int postId);
        bool CanDownload(HttpContext ctx, int postId);
    }
}
