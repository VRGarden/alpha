using Meilisearch;
using Microsoft.Extensions.Options;
using VRGardenAlpha.Models.Options;

namespace VRGardenAlpha
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddMeilisearch(this IServiceCollection services)
        {
            var options = services.BuildServiceProvider().GetRequiredService<IOptions<SearchOptions>>();
            var client = new MeilisearchClient(options.Value.Endpoint, options.Value.IndexKey);
            
            services.AddSingleton(client);
            return services;
        }
    }

    public class ImageProcessor
    {
        public static async Task ProcessImage(Stream incoming, string contentType, string path)
        {
            using var @is = await Image.LoadAsync(incoming);
            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                if (contentType == "image/gif")
                    await @is.SaveAsGifAsync(fs);
                else
                    await @is.SaveAsJpegAsync(fs);
            }
        }
    }
}
