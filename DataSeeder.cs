using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SafeVaultApp.Models;
using SafeVaultApp.Helpers;

public static class DataSeeder
{
    public static void SeedUsers(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        // context.Users.RemoveRange(context.Users); // Clear existing users for fresh seeding
        // context.SaveChanges();
        if (!context.Users.Any())
        {
            context.Users.AddRange(
                new User { Username = "admin", Password = PasswordHelper.HashPassword("SecurePass123!"), Role = "Admin" },
                new User { Username = "vaultuser", Password = PasswordHelper.HashPassword("Vault123!"), Role = "User" }
            );
            context.SaveChanges();
        }
    }
}
