using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using VRGardenAlpha.Data;
using VRGardenAlpha.Models.Options;
using VRGardenAlpha.Services.Analytics;
using VRGardenAlpha.Services.Security;

namespace VRGardenAlpha
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<SearchOptions>(builder.Configuration.GetSection("Meilisearch"));
            builder.Services.Configure<CaptchaOptions>(builder.Configuration.GetSection("Captcha"));
            builder.Services.Configure<GardenOptions>(builder.Configuration.GetSection("Garden"));
            builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));

            builder.Services.AddScoped<ICaptchaService, CloudflareCaptchaService>();
            builder.Services.AddScoped<IRemoteAddressService, RemoteAddressService>();
            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

            builder.Services.AddDbContext<GardenContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Main")));

            builder.Services.AddHttpClient();
            builder.Services.AddMemoryCache();
            builder.Services.AddMeilisearch();
            builder.Services.AddAutoMapper(typeof(VRClassMap));
            builder.Services.AddControllers();

            var app = builder.Build();

            var options = app.Services.GetService<IOptions<StorageOptions>>();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(options?.Value.MountPath!),
                ContentTypeProvider = new UnityContentTypeProvider(),
                ServeUnknownFileTypes = true,
                RequestPath = "/@storage"
            });

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}