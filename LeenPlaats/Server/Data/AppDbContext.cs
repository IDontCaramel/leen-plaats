using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ad> Ads => Set<Ad>();
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<LendRequest> LendRequests => Set<LendRequest>();
    public DbSet<NotifySubscription> NotifySubscriptions => Set<NotifySubscription>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Ad>(e =>
        {
            e.HasOne(a => a.Owner)
             .WithMany(u => u.Ads)
             .HasForeignKey(a => a.OwnerId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LendRequest>(e =>
        {
            e.HasOne(r => r.Ad)
             .WithMany(a => a.LendRequests)
             .HasForeignKey(r => r.AdId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.Requester)
             .WithMany(u => u.LendRequests)
             .HasForeignKey(r => r.RequesterId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<NotifySubscription>(e =>
        {
            e.HasIndex(n => new { n.AdId, n.UserId }).IsUnique();

            e.HasOne(n => n.Ad)
             .WithMany(a => a.NotifySubscriptions)
             .HasForeignKey(n => n.AdId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(n => n.User)
             .WithMany(u => u.NotifySubscriptions)
             .HasForeignKey(n => n.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
