using WarungKu.Models;

namespace WarungKu.Data
{
    public static class DbInitializer
    {
        public static void Initialize(WarungKuDbContext context)
        {

            context.Database.EnsureCreated();

            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User
                    {
                        Username = "admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Role = "Admin"
                    },
                    new User
                    {
                        Username = "employee",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Role = "Employee"
                    }
                    );
                context.SaveChanges();
            }

        }
    }
}
