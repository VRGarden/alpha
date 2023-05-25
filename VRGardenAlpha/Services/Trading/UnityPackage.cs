using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Text;

namespace VRGardenAlpha.Services.Trading
{
    public class UnityPackage
    {
        public static string? Extract(string src)
        {
            var dest = Directory.CreateTempSubdirectory("vrcg");

            try
            {
                using var stream = File.OpenRead(src);
                using var gzip = new GZipInputStream(stream);
                using var tar = TarArchive.CreateInputTarArchive(gzip, Encoding.UTF8);

                tar.ExtractContents(dest.FullName);
                tar.Close();
                gzip.Close();

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
