namespace apiTest.Data.CodeFirstMigration
{
    public class User 
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PasswordHash { get; set; }

        public string DateOfBirth { get; set; }
        public DateTime? InsertedDate { get; set; }
        public string? UpdatedDate { get; set; }
        public DateTime? Updatedby { get; set; }


    }
}
