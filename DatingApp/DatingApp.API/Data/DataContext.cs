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

        // OVERALL: Many-to-Many relationship between (Member) users and roles,
        // via 'MemberRoles' that acts as the join/linking table with two foreign keys: 'UserId' and 'RoleId'
        //
        // One-to-Many relationship (Member → MemberRole): One (Member) user can have many roles (through 'MemberRole' linking table)
        builder.Entity<Entities.Member>()
            .HasMany(m => m.MemberRoles)
            .WithOne(u => u.Member)
            .HasForeignKey(m => m.UserId)
            .IsRequired();
        // One-to-Many relationship (Role → MemberRole): One Role can be assigned to many (Members) users (through 'MemberRole' linking table)
        builder.Entity<Entities.Role>()
            .HasMany(m => m.MemberRoles)
            .WithOne(u => u.Role)
            .HasForeignKey(m => m.RoleId)
            .IsRequired();

        // OVERALL: Many-to-Many relationship (self-referencing) between a (Member) user and another (Member) user,
        // via 'UserLike' that acts as the join/linking table with two foreign keys: 'SourceUserId' and 'TargetUserId'
        builder.Entity<Entities.UserLike>()
            .HasKey(l => new { l.SourceUserId, l.TargetUserId });
        // One-to-Many relationship (Member → UserLike): One (Member) user can like many (Member) users (through 'UserLike' linking table)
        builder.Entity<Entities.UserLike>()
            .HasOne(l => l.SourceUser)
            .WithMany(u => u.LikesUsers)
            .HasForeignKey(l => l.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);
        // One-to-Many relationship (Member → UserLike): One (Member) user can be liked many (Member) users (through 'UserLike' linking table)
        builder.Entity<Entities.UserLike>()
            .HasOne(l => l.TargetUser)
            .WithMany(u => u.LikedByUsers)
            .HasForeignKey(l => l.TargetUserId)
            .OnDelete(DeleteBehavior.NoAction);

        // OVERALL: The 'Message' table/entity links a sender (Member) user and a recipient (Member) user
        // One-to-Many relationship (Member → Message): One (Member) user can receive many messages
        builder.Entity<Entities.Message>()
            .HasOne(m => m.Recipient)
            .WithMany(u => u.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);
        // One-to-Many relationship (Member → Message): One (Member) user can send many messages
        builder.Entity<Entities.Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
