using Mezzex_Inventory_Mangement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

                // Seed roles and pages
                await SeedRolesAndPagesAsync(context, logger);

                // Seed users and assign roles
                await SeedUsersAsync(userManager, roleManager, logger);

                // Assign default access for Administrator role
                await AssignDefaultAccessToAdministrator(context, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task SeedRolesAndPagesAsync(ApplicationDbContext context, ILogger logger)
        {
            using (var scope = context.Database.BeginTransaction())
            {
                try
                {
                    // Seed roles
                    if (!context.Roles.Any())
                    {
                        var adminRole = new ApplicationRole { Name = "Admin", CreatedBy = "Seed" };
                        var userRole = new ApplicationRole { Name = "User", CreatedBy = "Seed" };

                        context.Roles.AddRange(adminRole, userRole);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Roles seeded successfully.");
                    }

                    // Seed pages
                    var pages = new List<Page>
            {
                // Account pages
                new Page { Name = "Account - Access Denied", Url = "/Account/AccessDenied" },
                new Page { Name = "Account - Forgot Password", Url = "/Account/ForgotPassword" },
                new Page { Name = "Account - Login", Url = "/Account/Login" },
                new Page { Name = "Account - Logout", Url ="/Account/Logout"},
                new Page { Name = "Account - Reset Password", Url = "/Account/ResetPassword" },
                new Page { Name = "Account - Select Company", Url = "/Account/SelectCompany" },
                new Page { Name = "Account - Verify OTP", Url = "/Account/VerifyOtp" },

                // Home pages
                new Page { Name = "Home Index", Url = "/Home/Index" },
                new Page { Name = "Privacy", Url = "/Home/Privacy" },

                // ManageCompanies pages
                new Page { Name = "Manage Companies - Create", Url = "/ManageCompanies/Create" },
                new Page { Name = "Manage Companies - Delete", Url = "/ManageCompanies/Delete/*" },
                new Page { Name = "Manage Companies - Details", Url = "/ManageCompanies/Details/*" },
                new Page { Name = "Manage Companies - Edit", Url = "/ManageCompanies/Edit/*" },
                new Page { Name = "Manage Companies - Index", Url = "/ManageCompanies" },

                // PageManagement pages
                new Page { Name = "Page Management - Create Page", Url = "/PageManagement/CreatePage" },
                new Page { Name = "Page Management - Index", Url = "/PageManagement" },
                new Page { Name = "Page Management - Assign Pages To Roles", Url = "/PageManagement/AssignPagesToRoles/*" },

                new Page { Name = "Page Management - Toggle Page Role", Url = "/pagemanagement/TogglePageRoleMapping" },
                // Roles pages
                new Page { Name = "Roles - Create", Url = "/Roles/Create" },
                new Page { Name = "Roles - Delete", Url = "/Roles/Delete/*" },
                new Page { Name = "Roles - Edit", Url = "/Roles/Edit/*" },
                new Page { Name = "Roles - Index", Url = "/Roles" },

                // SellingChannels pages
                new Page { Name = "Selling Channels - Create", Url = "/SellingChannels/Create" },
                new Page { Name = "Selling Channels - Delete", Url = "/SellingChannels/Delete/*" },
                new Page { Name = "Selling Channels - Details", Url = "/SellingChannels/Details/*" },
                new Page { Name = "Selling Channels - Edit", Url = "/SellingChannels/Edit/*" },
                new Page { Name = "Selling Channels - Index", Url = "/SellingChannels" },

                //Users 
                new Page { Name = "Users - Create", Url = "/Users/Create" },
                new Page { Name = "Users - Delete", Url = "/Users/Delete/*" },
                new Page { Name = "Users - Details", Url = "/Users/Details/*" },
                new Page { Name = "Users - Edit", Url = "/Users/Edit/*" },
                new Page { Name = "Users - Index", Url = "/Users" },

                //assignCompany

                new Page { Name = "Assign Company - Create/Delete/Update", Url = "/AssignCompany/AssignCompanyToUser"},
  /*            new Page { Name = "Assign Company - Delete", Url = "/AssignCompany/Delete/*" },
                new Page { Name = "Assign Company - Details", Url = "/AssignCompany/Details/*" },
                new Page { Name = "Assign Company - Edit", Url = "/AssignCompany/Edit/*" },*/
                new Page { Name = "Assign Company - Index", Url = "/AssignCompany" }
            };

                    foreach (var page in pages)
                    {
                        if (!context.Pages.Any(p => p.Url == page.Url))
                        {
                            context.Pages.Add(page);
                            logger.LogInformation($"Page '{page.Name}' added.");
                        }
                    }

                    await context.SaveChangesAsync();
                    logger.LogInformation("Pages seeded successfully.");
                    await scope.CommitAsync();
                }
                catch (Exception ex)
                {
                    await scope.RollbackAsync();
                    logger.LogError($"Error seeding roles and pages: {ex.Message}");
                }
            }
        }

        private static async Task SeedUsersAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger logger)
        {
            await CreateUserAsync(
                userManager, roleManager, logger,
                "faizraza349@gmail.com", "Kausar@786",
                "Admin", "User", "Male", "India", "8052738480", new[] { "Admin" });
            await CreateUserAsync(
                userManager, roleManager, logger,
                "islam@direct-pharmacy.co.uk", "Sonaislam@143#",
                "Admin", "User", "Male", "India", "8707681811", new[] { "Admin" });
            await CreateUserAsync(
                userManager, roleManager, logger,
                "mezzexmaz@gmail.com", "Password123!",
                "User", "One", "Female", "Country", "0987654321", new[] { "User" });
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

        private static async Task AssignDefaultAccessToAdministrator(ApplicationDbContext context, ILogger logger)
        {
            // Ensure Administrator role exists
            var administratorRole = context.Roles.FirstOrDefault(r => r.Name == "Administrator");
            if (administratorRole == null)
            {
                logger.LogError("Administrator role not found. Unable to assign default access.");
                return;
            }

            // Get all pages
            var pages = context.Pages.ToList();

            // Assign all pages to the Admin role
            foreach (var page in pages)
            {
                if (!context.PageRoleMappings.Any(prm => prm.RoleId == administratorRole.Id && prm.PageId == page.Id))
                {
                    context.PageRoleMappings.Add(new PageRoleMapping
                    {
                        RoleId = administratorRole.Id,
                        PageId = page.Id
                    });
                }
            }

            // Allow all roles to access Login and Home pages
            var allRoles = context.Roles.ToList();
            var publicPages = pages.Where(p => p.Url == "/Account/Login" || p.Url == "/Home" || p.Url == "/Account/Logout" || p.Url == "/Account/ForgotPassword" || p.Url == "/Account/ResetPassword" || p.Url == "/Account/VerifyOtp" || p.Url == "/Account/AccessDenied").ToList();

            foreach (var role in allRoles)
            {
                foreach (var page in publicPages)
                {
                    if (!context.PageRoleMappings.Any(prm => prm.RoleId == role.Id && prm.PageId == page.Id))
                    {
                        context.PageRoleMappings.Add(new PageRoleMapping
                        {
                            RoleId = role.Id,
                            PageId = page.Id
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Default access assigned to Administrator and public pages.");
        }

    }
}
