using LibraryManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context)
        {
            // ensure DB created and migrations applied
            await context.Database.MigrateAsync();


            if (!context.Books.Any())
            {
                context.Books.AddRange(
                new Book { Title = "The Pragmatic Programmer", Author = "Andrew Hunt", ISBN = "978-0201616224", PublishedDate = new DateTime(1999, 10, 20) },
                new Book { Title = "Clean Code", Author = "Robert C. Martin", ISBN = "978-0132350884", PublishedDate = new DateTime(2008, 8, 1) },
                new Book { Title = "Domain-Driven Design", Author = "Eric Evans", ISBN = "978-0321125217", PublishedDate = new DateTime(2003, 8, 30) }
                );
                await context.SaveChangesAsync();
            }


            // seed a demo user (username: admin, password: Pa$$w0rd) if not exists
            if (!context.Users.Any())
            {
                var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
                var user = new User { Username = "admin" };
                user.PasswordHash = hasher.HashPassword(user, "Pa$$w0rd");
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }
        }
    }
}
