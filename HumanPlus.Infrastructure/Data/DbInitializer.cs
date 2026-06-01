using HumanPlus.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace HumanPlus.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRoles(RoleManager<ApplicationRole> roleManager)
        {
            string[] roleNames = ["Admin", "Employer", "JobSeeker", "SubAdmin", "Recruiter"];

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = roleName,
                        Description = roleName switch
                        {
                            "Admin" => "HumanPlus operations/admin team",
                            "Employer" => "Companies requesting manpower",
                            "JobSeeker" => "Skilled/Semi-skilled/Unskilled candidates",
                            "SubAdmin" => "Branch/operations support staff",
                            "Recruiter" => "HumanPlus recruitment executives",
                            _ => ""
                        }
                    });
                }
            }
        }

        public static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@humanplus.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
