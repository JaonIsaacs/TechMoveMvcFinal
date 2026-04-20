using Microsoft.AspNetCore.Identity;
using TechMove.Models;

namespace TechMove.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Create Roles
            string[] roles = { "Admin", "Manager" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create Admin User
            var adminEmail = "admin@techmove.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "TechMove Administrator",
                    EmailConfirmed = true,
                    CreatedDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(newAdmin, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            // Create Manager User
            var managerEmail = "manager@techmove.com";
            var managerUser = await userManager.FindByEmailAsync(managerEmail);

            if (managerUser == null)
            {
                var newManager = new ApplicationUser
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    FullName = "TechMove Manager",
                    EmailConfirmed = true,
                    CreatedDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(newManager, "Manager@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newManager, "Manager");
                }
            }
        }
    }
}
