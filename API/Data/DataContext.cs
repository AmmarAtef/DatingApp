using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext :
        IdentityDbContext<AppUser, AppRole,
        int, IdentityUserClaim<int>, AppUserRole,
        IdentityUserLogin<int>, IdentityRoleClaim<int>,
        IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options)
        {

        }


        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection>Connections { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            builder.Entity<AppRole>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .IsRequired();



            builder.Entity<UserLike>()
               .HasKey(k => new { k.SourceUserId, k.LikedUserId });

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

            builder.Entity<Message>()
                .HasOne(k => k.Recipient)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(u => u.Sender)
                .WithMany(o => o.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}