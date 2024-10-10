using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Quillry.Server.Domain;

namespace Quillry.Server.DataAccess
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        public DbSet<AppUserLogin> AppUserLogins { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
              .HasMany(x => x.Logins)
              .WithOne(x => x.User)
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AppUserLogin>(e =>
            {
                e.HasKey(k => k.Id);
                e.Property(p => p.IPAddress).IsRequired(true);
                e.Property(p => p.UserAgentInfo).IsRequired(true);
                e.Property(p => p.UserId).IsRequired(true);
            });
        }
    }
}
