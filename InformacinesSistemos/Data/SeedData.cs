using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InformacinesSistemos.Models;

namespace InformacinesSistemos.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var scoped = scope.ServiceProvider;

            var context = scoped.GetRequiredService<LibraryContext>();
            await context.Database.MigrateAsync();

            var userManager = scoped.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scoped.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure roles
            var adminRole = "Administrator";
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            // Admin user
            var config = scoped.GetRequiredService<IConfiguration>();
            var adminEmail = config["AdminUser:Email"] ?? "admin@local";
            var adminPassword = config["AdminUser:Password"] ?? "Admin123!";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, adminRole);
                }
                else
                {
                    // If creation failed, stop here so we don't attempt profile creation with null id.
                    return;
                }
            }

            // Ensure UserAccount row exists for this Identity user (maps to existing user_accounts table)
            var profile = await context.UserAccounts.FirstOrDefaultAsync(p => p.IdentityUserId == admin.Id);
            if (profile == null)
            {
                profile = new UserAccount
                {
                    IdentityUserId = admin.Id,
                    FirstName = "Admin",
                    LastName = "User",
                    Email = admin.Email,
                    PhoneNumber = admin.PhoneNumber,
                    RegistrationDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Role = Models.Enums.UserRole.Administrator
                };

                context.UserAccounts.Add(profile);
                await context.SaveChangesAsync();
            }
        }
    }
}