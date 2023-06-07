namespace VRGardenAlpha.Services.Trading
{
    public interface IPackageInspectorService
    {
        Task<string[]> GetPackagePathsAsync(string path);
    }
}
