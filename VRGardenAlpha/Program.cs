using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using VRGardenAlpha.Data;
using VRGardenAlpha.Models.Options;
using VRGardenAlpha.Services.Analytics;
using VRGardenAlpha.Services.Security;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

            builder.Services.AddCors();
            builder.Services.AddHttpClient();
            builder.Services.AddMemoryCache();
            builder.Services.AddMeilisearch();
            builder.Services.AddAutoMapper(typeof(VRClassMap));
            builder.Services.AddControllers();

            var app = builder.Build();
            string? cors = builder.Configuration["Garden:CorsDomains"];
            string[]? domains = cors?.Split(',').Select(x => x.Trim()).ToArray();

            app.UseCors(x => x
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithOrigins(domains ?? Array.Empty<string>()));

            Console.WriteLine("Using CORS domains: " + cors);

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

            app.MapFallback(async (ctx) =>
            {
                var error = new
                {
                    Type = "ENDPOINT_NOT_FOUND",
                    Title = $"The requested resource '{ctx.Request.Path}' does not exist on the server.",
                    TraceId = Activity.Current?.Id ?? ctx.TraceIdentifier,
                    Status = 404
                };

                ctx.Response.StatusCode = 404;
                await ctx.Response.WriteAsJsonAsync(error);
            });

            app.Run();
            // Run the application
        }
    }
}