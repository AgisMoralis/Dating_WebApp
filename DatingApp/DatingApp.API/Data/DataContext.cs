using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data;

public class DataContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Entities.Member> Members { get; set; }
    public DbSet<Entities.UserLike> Likes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Entities.UserLike>()
            .HasKey(l => new { l.SourceUserId, l.TargetUserId });

        builder.Entity<Entities.UserLike>()
            .HasOne(l => l.SourceUser)
            .WithMany(u => u.LikesUsers)
            .HasForeignKey(l => l.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Entities.UserLike>()
            .HasOne(l => l.TargetUser)
            .WithMany(u => u.LikedByUsers)
            .HasForeignKey(l => l.TargetUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
