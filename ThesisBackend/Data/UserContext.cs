using Microsoft.EntityFrameworkCore;
using ThesisBackend.Models.UserModels;

namespace ThesisBackend.Data
{
    public class UserContext(DbContextOptions<UserContext> options) : DbContext(options)
    {
        public virtual required DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.FirstName).IsRequired();
                entity.Property(e => e.LastName).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.Role).IsRequired();
            });
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("Role")
                .HasValue<Student>("student")
                .HasValue<Teacher>("teacher")
                .HasValue<Parent>("parent")
                .HasValue<Admin>("admin");
        }
    }
}
