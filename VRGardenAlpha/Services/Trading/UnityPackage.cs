using ICSharpCode.SharpZipLib.GZip;
using System.Formats.Tar;
using System.Text;

namespace VRGardenAlpha.Services.Trading
{
    public class UnityPackage
    {
        public static async Task<string?> Clean(string src)
        {
            try
            {
                using var stream = File.OpenRead(src);
                using var gzip = new GZipInputStream(stream);
                using var tar = new TarReader(gzip);

                TarEntry? entry;
                List<TarEntry> entries = new List<TarEntry>();
                List<TarEntry> remove = new List<TarEntry>();
                while((entry = await tar.GetNextEntryAsync()) != null)
                    entries.Add(entry);

                var final = Path.GetTempFileName();
                foreach(var e in entries)
                {
                    if(e.Name.EndsWith("pathname"))
                    {
                        var path = Path.GetTempFileName();
                        await e.ExtractToFileAsync(path, true);

                        var data = File.ReadAllText(path);
                        if(data.EndsWith("RuntimeCrashHandler.cs") 
                            || data.EndsWith("VRCSDK.Analytics.dll") 
                            || data.EndsWith("VRCSDK.Core.dll") 
                            || data.EndsWith("VRCSDK.Editor.dll"))
                        {
                            var dir = Path.GetDirectoryName(e.Name);
                            var asset = dir + "/asset";
                            var meta = dir + "/meta";
                            var pathname = dir + "/pathname";

                            var assetEntry = entries.FirstOrDefault(x => x.Name == asset);
                            var metaEntry = entries.FirstOrDefault(x => x.Name == meta);
                            var pathnameEntry = entries.FirstOrDefault(x => x.Name == pathname);

                            if (assetEntry != null)
                                remove.Add(assetEntry);

                            if (metaEntry != null)
                                remove.Add(metaEntry);

                            if (pathnameEntry != null)
                                remove.Add(pathnameEntry);
                        }
                    }
                }

                using var outstream = File.OpenWrite(final);
                using var gzipOut = new GZipOutputStream(stream);
                using var tarOut = new TarWriter(gzipOut);
                foreach (var en in entries)
                    if (remove.Contains(en))
                        continue;
                    else
                        await tarOut.WriteEntryAsync(en);

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
