using Microsoft.EntityFrameworkCore;

namespace TrackingService.Services.Db
{
    public class TrackingServiceDbContext : DbContext
    {
        public TrackingServiceDbContext(DbContextOptions<TrackingServiceDbContext> options)
            : base(options)
        {
        }
    }
}
