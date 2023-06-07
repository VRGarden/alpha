using ICSharpCode.SharpZipLib.GZip;
using System.Formats.Tar;

namespace VRGardenAlpha.Services.Trading
{
    public class UnityPackage
    {
        public static async Task<string?> Extract(string src)
        {
            var dest = Directory.CreateTempSubdirectory("vrcg");

            try
            {
                using var stream = File.OpenRead(src);
                using var gzip = new GZipInputStream(stream);
                using var tar = new TarReader(gzip);

                TarEntry? entry;

                do
                {
                    entry = await tar.GetNextEntryAsync();
                    if (entry == null)
                        break;

                    if (entry.Name.EndsWith("pathname"))
                    {
                        string path = Path.Combine(dest.FullName, Guid.NewGuid().ToString() + "/pathname");
                        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                        
                        await entry.ExtractToFileAsync(path, true);
                    }
                }
                while (entry != null);

                tar.Dispose();
                return dest.FullName;
            }
            catch
            {
                dest.Delete();
                return null;
            }
        }

        public static async IAsyncEnumerable<string> GetAllFiles(string packagePath)
        {
            foreach (var dir in Directory.GetDirectories(packagePath))
                yield return await File.ReadAllTextAsync(Path.Combine(dir, "pathname"));
        }
    }
}
