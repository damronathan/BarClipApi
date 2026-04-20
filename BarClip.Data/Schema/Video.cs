using BarClip.Models.Base;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarClip.Data.Schema;

public class Video : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid SessionId { get; set; }
    public Session? Session { get; set; }
    public int OrderNumber { get; set; }
    public bool IsFull { get; set; }

    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Video>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.HasOne(o => o.User)
                .WithMany(u => u.Videos)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(o => o.Session)
                  .WithMany(s => s.Videos)
                  .HasForeignKey(l => l.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}