using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<AppUser>Users{ get; set; }
        public DbSet<UserLike> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserLike>()
               .HasKey(k => new { k.SourceUserId, k.LikedUserId});

            builder.Entity<UserLike>()
                .HasOne(c => c.LikedUser)
                .WithMany(j => j.LikedByUsers)
                .HasForeignKey(k => k.LikedUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<UserLike>()
               .HasOne(c => c.SourceUser)
               .WithMany(j => j.LikedUsers)
               .HasForeignKey(k => k.SourceUserId)
               .OnDelete(DeleteBehavior.NoAction);
        }
    }
}