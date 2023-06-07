namespace VRGardenAlpha.Services.Trading
{
    public interface IPackageInspectorService
    {
        Task CleanPackageAsync(string path);
        Task<string[]> GetPackagePathsAsync(string path);
    }
}
