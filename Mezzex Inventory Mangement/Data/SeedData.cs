using Mezzex_Inventory_Mangement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mezzex_Inventory_Mangement.Data
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var logger = serviceProvider.GetRequiredService<ILogger<SeedData>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                logger.LogInformation("Starting data seeding...");

                // Seed roles
                await SeedRolesAsync(roleManager, logger);

                // Seed permissions
                await SeedPermissionsAsync(context, logger);

                // Seed users and assign roles and permissions
                await SeedUsersAsync(userManager, roleManager, logger, context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
        {
            string[] roleNames = { "User", "Admin", "Administrator", "Account" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Role '{roleName}' created successfully.");
                    }
                    else
                    {
                        logger.LogError($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }


        private static async Task SeedPermissionsAsync(ApplicationDbContext context, ILogger logger)
        {
            if (!context.PermissionsName.Any())
            {
                var permissions = new[]
                {
            new PermissionName { Name = "View", Page = "Users" },
            new PermissionName { Name = "Add", Page = "Users" },
            new PermissionName { Name = "Edit", Page = "Users" },
            new PermissionName { Name = "Delete", Page = "Users" },

            new PermissionName { Name = "View", Page = "Company" },
            new PermissionName { Name = "Manage", Page = "Company" },

            new PermissionName { Name = "View", Page = "SellingChannel" },
            new PermissionName { Name = "Manage", Page = "SellingChannel" }
        };

                await context.PermissionsName.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
                logger.LogInformation("Permissions seeded successfully.");
            }
        }


        private static async Task SeedUsersAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger logger,
            ApplicationDbContext context)
        {
            await CreateUserAsync(
                userManager, roleManager, logger,
                "faizraza349@gmail.com", "Kausar@786",
                "Admin", "User", "Male", "India", "8052738480", new[] { "Administrator" });
            await CreateUserAsync(
                userManager, roleManager, logger,
                "islam@direct-pharmacy.co.uk", "Sonaislam@143#",
                "Admin", "User", "Male", "India", "8707681811", new[] { "Administrator" });
            await CreateUserAsync(
                userManager, roleManager, logger,
                "mezzexmaz@gmail.com", "Password123!",
                "User", "One", "Female", "Country", "0987654321", new[] { "User" });

            await AssignAllPermissionsToAdministrator(userManager, context, logger);
        }

        private static async Task CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger logger,
            string email,
            string password,
            string firstName,
            string lastName,
            string gender,
            string country,
            string phonenumber,
            string[] roles)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Gender = gender,
                    Active = true,
                    PhoneNumber = phonenumber,
                    CountryName = country,
                };

                var createUserResult = await userManager.CreateAsync(user, password);
                if (createUserResult.Succeeded)
                {
                    logger.LogInformation($"User '{email}' created successfully.");

                    foreach (var role in roles)
                    {
                        if (await roleManager.RoleExistsAsync(role))
                        {
                            await userManager.AddToRoleAsync(user, role);
                            logger.LogInformation($"Role '{role}' assigned to user '{email}'.");
                        }
                    }

                    if (roles.Contains("Administrator"))
                    {
                        await AssignClaimsToUser(userManager, user, new[] { "Manage Company", "Manage Selling Channel", "View Assigned Company", "View Assigned Selling Channel" });
                    }
                }
                else
                {
                    logger.LogError($"Failed to create user '{email}': {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation($"User '{email}' already exists.");
            }
        }

        private static async Task AssignClaimsToUser(UserManager<ApplicationUser> userManager, ApplicationUser user, string[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (!(await userManager.GetClaimsAsync(user)).Any(c => c.Type == "Permission" && c.Value == permission))
                {
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Permission", permission));
                }
            }
        }

        private static async Task AssignAllPermissionsToAdministrator(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger logger)
        {
            var administrator = await userManager.FindByEmailAsync("faizraza349@gmail.com");
            if (administrator != null)
            {
                var permissions = context.PermissionsName.Select(p => p.Name).ToArray();
                await AssignClaimsToUser(userManager, administrator, permissions);
                logger.LogInformation("All permissions assigned to Administrator.");
            }
        }
    }
}
