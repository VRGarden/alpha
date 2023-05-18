using Microsoft.EntityFrameworkCore;

namespace VRGardenAlpha.Data
{
    public class GardenContext : DbContext
    {
        #pragma warning disable 8618
        public GardenContext(DbContextOptions<GardenContext> options) : base(options) { }
        #pragma warning restore 8618
        
        public DbSet<Post> Posts { get; set; }
    }
}
