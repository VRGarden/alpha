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
}
