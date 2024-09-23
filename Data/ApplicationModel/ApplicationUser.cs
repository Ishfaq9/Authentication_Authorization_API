using apiTest.Data.CodeFirstMigration;
using Microsoft.AspNetCore.Identity;

namespace apiTest.Data.ApplicationModel
{
    public class ApplicationUser: IdentityUser
    {
        public ApplicationUser() { }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
