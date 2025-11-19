using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InformacinesSistemos.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<UserProfile> UserProfiles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Map UserProfile to existing user_account table and key
            builder.Entity<UserProfile>(b =>
            {
                b.ToTable("user_account"); // Postgres: unquoted lowercase
                b.HasKey(p => p.UserId);
                b.HasOne(p => p.User)
                 .WithOne(u => u.Profile)
                 .HasForeignKey<UserProfile>(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // If your existing table has different column names adjust Column mappings or Fluent API above.
        }
    }
}