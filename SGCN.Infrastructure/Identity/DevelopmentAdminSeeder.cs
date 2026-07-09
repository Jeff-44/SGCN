using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGCN.Domain.Constants;
using SGCN.Domain.Identity;
using SGCN.Infrastructure.Persistence;

namespace SGCN.Infrastructure.Identity;

public static class DevelopmentAdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var options = configuration.GetSection("SeedAdmin").Get<SeedAdminOptions>();
        if (options is null ||
            string.IsNullOrWhiteSpace(options.Email) ||
            string.IsNullOrWhiteSpace(options.UserName) ||
            string.IsNullOrWhiteSpace(options.FullName) ||
            string.IsNullOrWhiteSpace(options.Password))
        {
            return;
        }

        using var scope = services.CreateScope();

        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (!await dbContext.Database.CanConnectAsync())
            {
                return;
            }

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var existingUser = await userManager.FindByEmailAsync(options.Email.Trim());

            if (existingUser is not null)
            {
                return;
            }

            var user = new ApplicationUser
            {
                Email = options.Email.Trim(),
                UserName = options.UserName.Trim(),
                FullName = options.FullName.Trim(),
                EmailConfirmed = true,
                IsActive = true,
                ForcePasswordChange = false
            };

            var createResult = await userManager.CreateAsync(user, options.Password);
            if (!createResult.Succeeded)
            {
                return;
            }

            await userManager.AddToRoleAsync(user, SystemRoles.Administrateur);
        }
        catch (Exception)
        {
            // Development seed issues should not prevent API health checks or Swagger from loading.
        }
    }

    private sealed class SeedAdminOptions
    {
        public string Email { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
