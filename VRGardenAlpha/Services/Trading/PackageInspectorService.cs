namespace VRGardenAlpha.Services.Trading
{
    public class PackageInspectorService : IPackageInspectorService
    {
        public async Task CleanPackageAsync(string path)
        {
            var cleaned = await UnityPackage.Clean(path);
            if (cleaned != null)
                File.Move(cleaned, path, true);
        }

        public async Task<string[]> GetPackagePathsAsync(string path)
        {
            string package = await UnityPackage.Extract(path) ?? throw new Exception("Failed to extract Unity package.");
            var files = await UnityPackage.GetAllFiles(package).ToArrayAsync();

            Directory.Delete(package, true);
            return files;
        }
    }
}
