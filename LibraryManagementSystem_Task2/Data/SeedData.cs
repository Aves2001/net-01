using LibraryManagementSystem_Task2.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem_Task2.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new LibraryContext(
                serviceProvider.GetRequiredService<DbContextOptions<LibraryContext>>());

            if (context.Users.Any())
            {
                return; // Дані вже є
            }

            context.Users.AddRange(
                new User { Name = "John Doe", Email = "john@example.com", Phone = "123-456-7890" },
                new User { Name = "Jane Smith", Email = "jane@example.com", Phone = "987-654-3210" }
            );

            context.Books.AddRange(
                new Book { Title = "C# Programming", Author = "John Sharp", Available = 3 },
                new Book { Title = "ASP.NET Core", Author = "Dino Esposito", Available = 5 }
            );

            context.SaveChanges();
        }
    }
}
