using apiTest.Data.ApplicationModel;
using apiTest.Data.CodeFirstMigration;
using apiTest.Data.Initializer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace apiTest.Data.DataAccess
{
    public class UserDbContext : IdentityDbContext<ApplicationUser>
    {
        public UserDbContext(DbContextOptions options) : base(options)
        {
        }
        public virtual DbSet<User>  Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            DbInitializer.SeedData(modelBuilder);
        }
    }
}
