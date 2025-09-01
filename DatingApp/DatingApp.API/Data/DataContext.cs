using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data;

public class DataContext(DbContextOptions options) :
    IdentityDbContext<
        Entities.Member, Entities.Role, int,
        IdentityUserClaim<int>, Entities.MemberRole,
        IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>(options)
{
    public DbSet<Entities.UserLike> Likes { get; set; }
    public DbSet<Entities.Message> Messages { get; set; }
    public DbSet<Entities.Group> Groups { get; set; }
    public DbSet<Entities.Connection> Connections { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Entities.Member>()
            .HasMany(m => m.MemberRoles)
            .WithOne(u => u.Member)
            .HasForeignKey(m => m.UserId)
            .IsRequired();

        builder.Entity<Entities.Role>()
            .HasMany(m => m.MemberRoles)
            .WithOne(u => u.Role)
            .HasForeignKey(m => m.RoleId)
            .IsRequired();

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
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Entities.Message>()
            .HasOne(m => m.Recipient)
            .WithMany(u => u.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Entities.Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
