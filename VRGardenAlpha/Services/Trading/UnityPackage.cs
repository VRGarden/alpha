using ICSharpCode.SharpZipLib.GZip;
using System.Formats.Tar;

namespace VRGardenAlpha.Services.Trading
{
    public class UnityPackage
    {
        public static async Task<string?> Clean(string src)
        {
            try
            {
                var final = Path.GetTempFileName();
                using var stream = File.OpenRead(src);
                using var gzip = new GZipInputStream(stream);
                using var tar = new TarReader(gzip);
                using var outstream = File.OpenWrite(final);
                using var gzipOut = new GZipOutputStream(outstream);
                using var tarOut = new TarWriter(gzipOut);

                TarEntry? entry;
                List<string> entrySkip = new List<string>();
                while ((entry = await tar.GetNextEntryAsync()) != null)
                {
                    if (entrySkip.Any(x => entry.Name.Contains(x)))
                        continue;

                    bool add = true;
                    if (entry.Name.EndsWith("pathname"))
                    {
                        var path = Path.GetTempFileName();
                        await entry.ExtractToFileAsync(path, true);

                        var data = File.ReadAllText(path);
                        if (data.EndsWith("RuntimeCrashHandler.cs")
                            || data.EndsWith("VRCSDK.Analytics.dll")
                            || data.EndsWith("VRCSDK.Core.dll")
                            || data.EndsWith("VRCSDK.Editor.dll"))
                        {
                            var dir = Path.GetDirectoryName(entry.Name);
                            entrySkip.Add(dir!);
                            add = false;
                        }

                        File.Delete(path);

                        if (add)
                            await tarOut.WriteEntryAsync(entry);
                    }
                }
                
                tarOut.Dispose();
                tar.Dispose();

                return final;
            }
            catch(Exception exception)
            {
                Console.WriteLine(exception.ToString());
                return null;
            }
        }
        
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
