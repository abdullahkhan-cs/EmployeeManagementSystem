using Microsoft.AspNetCore.Identity;
namespace EMS.Web.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "HR", "Employee" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Admin account
            var adminEmail = "admin@ems.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Test HR account
            var hrEmail = "hr@ems.com";
            if (await userManager.FindByEmailAsync(hrEmail) == null)
            {
                var hrUser = new ApplicationUser
                {
                    UserName = hrEmail,
                    Email = hrEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(hrUser, "Hr@12345");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(hrUser, "HR");
            }

            // Test Employee account
            var empEmail = "employee@ems.com";
            if (await userManager.FindByEmailAsync(empEmail) == null)
            {
                var empUser = new ApplicationUser
                {
                    UserName = empEmail,
                    Email = empEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(empUser, "Employee@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(empUser, "Employee");
            }
        }
    }
}