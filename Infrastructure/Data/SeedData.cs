using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public static class SeedData
    {
        /// <summary>
        /// Ensures roles and an admin user exist.
        /// Reads admin credentials from configuration keys:
        ///   AdminUser:UserName, AdminUser:Email, AdminUser:Password
        /// Falls back to defaults when keys are missing (development only).
        /// </summary>
        public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Read from configuration (secure this in production)
            var adminUserName = configuration["AdminUser:UserName"] ?? "admin";
            var adminEmail = configuration["AdminUser:Email"] ?? "admin@giathuocthat.vn";
            var adminPassword = configuration["AdminUser:Password"] ?? "Admin@123"; // replace in production
            var adminRole = "Admin";

            try
            {
                // Ensure role exists
                if (!await roleManager.RoleExistsAsync(adminRole))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(adminRole));
                    if (!roleResult.Succeeded)
                        logger.LogError("Failed to create role {Role}: {Errors}", adminRole, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }

                // Ensure admin user exists
                var admin = await userManager.FindByNameAsync(adminUserName);
                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = adminUserName,
                        Email = adminEmail,
                        FullName = "Administrator",
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(admin, adminPassword);
                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        logger.LogError("Failed to create admin user: {Errors}", errors);
                        throw new InvalidOperationException($"Failed to create admin user: {errors}");
                    }
                }

                // Ensure user is in role
                if (!await userManager.IsInRoleAsync(admin, adminRole))
                {
                    var addRoleResult = await userManager.AddToRoleAsync(admin, adminRole);
                    if (!addRoleResult.Succeeded)
                        logger.LogError("Failed to add admin user to role {Role}: {Errors}", adminRole, string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                }

                logger.LogInformation("Seeding completed. Admin: {AdminUser}", adminUserName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred when seeding the database.");
                throw;
            }
        }
    }
}
