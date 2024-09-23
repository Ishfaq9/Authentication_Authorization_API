using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace apiTest.Data.Initializer
{
    internal class DbInitializer
    {
        internal static void SeedData(ModelBuilder modelBuilder)
        {
            SeedApplicationRoles(modelBuilder);
        }

        private static void SeedApplicationRoles(ModelBuilder modelBuilder) 
        {
            var indentityRoles = new List<IdentityRole>
            {
                new IdentityRole() {Id ="6B523E27-22FD-4EF2-95EF-2F087D8028AF",Name="Admin", ConcurrencyStamp ="1" ,NormalizedName="ADMIN"},
                new IdentityRole() {Id ="7B523E27-22FD-4EF2-95EF-2F087D8028AE",Name="User", ConcurrencyStamp ="2" ,NormalizedName="USER"},
            };
            modelBuilder.Entity<IdentityRole>().HasData(indentityRoles);
        }
    }
}
