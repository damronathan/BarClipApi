using BarClipApi.Data.Schema;
using Microsoft.EntityFrameworkCore;
namespace BarClipApi.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Video> Videos { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        Video.Configure(modelBuilder);
        User.Configure(modelBuilder);
        Session.Configure(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }
}

