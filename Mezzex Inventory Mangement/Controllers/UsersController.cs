﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Mezzex_Inventory_Mangement.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Mezzex_Inventory_Mangement.ViewModels;
using Mezzex_Inventory_Mangement.Data;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using Microsoft.AspNetCore.Authorization;

namespace Mezzex_Inventory_Mangement.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            this._context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user); // Await the async method

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Active = user.Active,
                    Roles = roles.ToList(), // Convert roles to a list
                    CreatedBy = user.CreatedBy,
                    CreatedOn = user.CreatedOn,
                    ModifiedBy = user.ModifiedBy,
                    ModifiedOn = user.ModifiedOn,
                    DateOfBirth = user.DateOfBirth
                });
            }

            return View(userViewModels);
        }

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User); // Get the logged-in user
            var userRoleIds = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            // Get the roles of the logged-in user
            var userRoles = await _roleManager.Roles
                .Where(r => userRoleIds.Contains(r.Id))
                .ToListAsync();

            // Determine the highest role of the logged-in user
            var isAdministrator = userRoles.Any(r => r.Name == "Administrator");
            var isAdmin = userRoles.Any(r => r.Name == "Admin");

            // Fetch roles based on hierarchy
            List<ApplicationRole> roles; // Change type to match ApplicationRole
            if (isAdministrator)
            {
                // If the user is Administrator, they can see and assign all roles
                roles = await _roleManager.Roles.ToListAsync();
            }
            else if (isAdmin)
            {
                // If the user is Admin, exclude the Administrator role
                roles = await _roleManager.Roles
                    .Where(r => r.Name != "Administrator")
                    .ToListAsync();
            }
            else
            {
                // Otherwise, restrict to roles below the user's role (if hierarchy applies)
                roles = await _roleManager.Roles
                    .Where(r => r.Name != "Administrator" && r.Name != "Admin")
                    .ToListAsync();
            }

            ViewBag.Roles = roles;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser model, string password, List<string> selectedRoles, IFormFile profileImage)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

            if (ModelState.IsValid)
            {
                // 2. Validate that the user can only assign roles they are allowed to manage
                if (currentUserRoles.Contains("Admin"))
                {
                    // Admin can only assign "Admin" or roles below it
                    var allowedRoles = await _roleManager.Roles
                        .Where(r => r.Name == "Admin" || r.Name != "Administrator") // Filter roles Admin can assign
                        .Select(r => r.Name)
                        .ToListAsync();

                    if (selectedRoles.Any(role => !allowedRoles.Contains(role)))
                    {
                        ModelState.AddModelError("", "You cannot assign roles higher than your access level.");
                        ViewBag.Roles = allowedRoles;
                        return View(model);
                    }
                }
                else if (currentUserRoles.Contains("Administrator"))
                {
                    // Administrator can assign any role (but logic can be adjusted if necessary)
                    var allowedRoles = await _roleManager.Roles.ToListAsync();
                    ViewBag.Roles = allowedRoles;
                }
                else
                {
                    // Unauthorized user, just in case
                    return Forbid();
                }

                // Check if the phone number already exists
                var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
                if (existingUser != null)
                {
                    ModelState.AddModelError("PhoneNumber", "This phone number is already registered.");
                    ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                    return View(model);
                }

                // Profile Image Upload Logic
                string imageUrl = null;
                if (profileImage != null && profileImage.Length > 0)
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("https://smapi.mezzex.com/api/UploadData/");
                        var formData = new MultipartFormDataContent();

                        var fileContent = new StreamContent(profileImage.OpenReadStream());
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(profileImage.ContentType);
                        formData.Add(fileContent, "file", profileImage.FileName);

                        var response = await client.PostAsync("upload-Images", formData);
                        if (response.IsSuccessStatusCode)
                        {
                            var result1 = await response.Content.ReadFromJsonAsync<UploadResponse>();
                            if (result1 != null && !string.IsNullOrEmpty(result1.FileName))
                            {
                                imageUrl = $"https://sm.mezzex.com/ScreenShot/{result1.FileName}";
                            }
                            else
                            {
                                ModelState.AddModelError("", "Failed to upload the profile image. Please try again.");
                                ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                                return View(model);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Error while uploading the image to the server.");
                            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                            return View(model);
                        }
                    }
                }

                // Create the user object
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Gender = model.Gender,
                    Active = model.Active,
                    CountryName = model.CountryName,
                    CreatedOn = DateTime.Now,
                    CreatedBy = $"{currentUser.FirstName} {currentUser.LastName}",
                    PhoneNumber = model.PhoneNumber,
                    DateOfBirth = model.DateOfBirth,
                    ProfileImageUrl = imageUrl
                };

                // Create the user in the Identity system
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    // Assign selected roles to the user
                    if (selectedRoles != null)
                    {
                        foreach (var role in selectedRoles)
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }

                    TempData["SuccessMessage"] = "User created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                // Handle errors during user creation
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Return the view with roles if something went wrong
            ViewBag.Roles = currentUserRoles.Contains("Admin")
                ? await _roleManager.Roles.Where(r => r.Name == "Admin" || r.Name != "Administrator").ToListAsync()
                : await _roleManager.Roles.ToListAsync();

            return View(model);
        }


        // Helper class to handle the image upload API response
        public class UploadResponse
        {
            public string FileName { get; set; }
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var currentuser = await _userManager.GetUserAsync(User); // Get the logged-in user
            var userRoleIds = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            // Get the roles of the logged-in user
            var CurrentuserRoles = await _roleManager.Roles
                .Where(r => userRoleIds.Contains(r.Id))
                .ToListAsync();

            // Determine the highest role of the logged-in user
            var isAdministrator = CurrentuserRoles.Any(r => r.Name == "Administrator");
            var isAdmin = CurrentuserRoles.Any(r => r.Name == "Admin");

            // Fetch roles based on hierarchy
            List<ApplicationRole> roles; // Change type to match ApplicationRole
            if (isAdministrator)
            {
                // If the user is Administrator, they can see and assign all roles
                roles = await _roleManager.Roles.ToListAsync();
            }
            else if (isAdmin)
            {
                // If the user is Admin, exclude the Administrator role
                roles = await _roleManager.Roles
                    .Where(r => r.Name != "Administrator")
                    .ToListAsync();
            }
            else
            {
                // Otherwise, restrict to roles below the user's role (if hierarchy applies)
                roles = await _roleManager.Roles
                    .Where(r => r.Name != "Administrator" && r.Name != "Admin")
                    .ToListAsync();
            }

            ViewBag.Roles = roles;
            ViewBag.UserRoles = userRoles;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ApplicationUser model, List<string> selectedRoles, IFormFile profileImage)
        {
            ModelState.Remove("profileImage"); // Optional fields
            var user = await _userManager.FindByIdAsync(id.ToString());
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserName = $"{currentUser.FirstName} {currentUser.LastName}";
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "User not found.";
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return NotFound();
                }

                // Check if the phone number already exists for another user
                var existingUserWithPhone = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber && u.Id != id);
                if (existingUserWithPhone != null)
                {
                    ModelState.AddModelError("PhoneNumber", "This phone number is already registered.");
                    var userRoles = await _userManager.GetRolesAsync(user);

                    ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                    ViewBag.UserRoles = userRoles;
                    return View(user);
                }

                // Update user properties
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Gender = model.Gender;
                user.Active = model.Active;
                user.CountryName = model.CountryName;
                user.ModifiedBy = currentUserName;
                user.ModifiedOn = DateTime.UtcNow;
                user.DateOfBirth = model.DateOfBirth;
                // Profile Image Upload Logic
                if (profileImage != null && profileImage.Length > 0)
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri("https://smapi.mezzex.com/api/UploadData/");
                        var formData = new MultipartFormDataContent();

                        var fileContent = new StreamContent(profileImage.OpenReadStream());
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(profileImage.ContentType);
                        formData.Add(fileContent, "file", profileImage.FileName);

                        var response = await client.PostAsync("upload-Images", formData);
                        if (response.IsSuccessStatusCode)
                        {
                            var result1 = await response.Content.ReadFromJsonAsync<UploadResponse>();
                            if (result1 != null && !string.IsNullOrEmpty(result1.FileName))
                            {
                                user.ProfileImageUrl = $"https://sm.mezzex.com/ScreenShot/{result1.FileName}";
                            }
                            else
                            {
                                ModelState.AddModelError("", "Failed to upload the profile image. Please try again.");
                                var userRoles = await _userManager.GetRolesAsync(user);

                                ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                                ViewBag.UserRoles = userRoles;
                                return View(user);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Error while uploading the image to the server.");
                            var userRoles = await _userManager.GetRolesAsync(user);

                            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                            ViewBag.UserRoles = userRoles;
                            return View(user);
                        }
                    }
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Update roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
                    var rolesToRemove = currentRoles.Except(selectedRoles).ToList();

                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                    TempData["SuccessMessage"] = "User updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int id)
        {
           
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                TempData["ErrorMessage"] = "User  not found.";
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userDetailsViewModel = new UserDetailsViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Active = user.Active,
                CountryName = user.CountryName,
                Roles = roles.ToList(),
                CreatedBy = user.CreatedBy,
                CreatedOn = user.CreatedOn,
                ModifiedBy = user.ModifiedBy,
                ModifiedOn = user.ModifiedOn,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                ProfileImageUrl = user.ProfileImageUrl,
                PhoneNumber = user.PhoneNumber
            };

            return View(userDetailsViewModel);
        }
        public async Task<IActionResult> ExportToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Fetch User and Assignment data
            var userAssignments = await _context.UserCompanyAssignments
                .Include(uca => uca.Company)
                    .ThenInclude(company => company.SellingChannels) // Include SellingChannels
                .Include(uca => uca.User) // Include User details
                .ToListAsync();

            var users = await _userManager.Users.ToListAsync();

            var userDetails = users.GroupJoin(
                userAssignments,
                u => u.Id,
                ua => ua.UserId,
                (user, assignments) => new UserDetailsViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Active = user.Active,
                    CountryName = user.CountryName,
                    Roles = _userManager.GetRolesAsync(user).Result.ToList(),
                    CreatedBy = user.CreatedBy,
                    CreatedOn = user.CreatedOn,
                    ModifiedBy = user.ModifiedBy,
                    ModifiedOn = user.ModifiedOn,
                    Gender = user.Gender,
                    DateOfBirth = user.DateOfBirth,
                    PhoneNumber = user.PhoneNumber,
                    ProfileImageUrl = user.ProfileImageUrl,
                    AssignedCompanies = assignments.Select(ua => new CompanyDetailsViewModel
                    {
                        CompanyName = ua.Company?.CompanyName ?? "N/A",
                        SellingChannels = ua.Company?.SellingChannels?.Select(sc => sc.SellingChannelName).ToList()
                                      ?? new List<string>()
                    }).ToList()
                }).ToList();

            // Generate Excel
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");

                // Add headers
                var headers = new[]
                {
            "Id", "First Name", "Last Name", "Email", "Active", "Country Name", "Roles",
            "Created By", "Created On", "Modified By", "Modified On", "Gender",
            "Date of Birth", "ProfileImageUrl", "Phone Number", "Company Details"
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Add user data
                for (int i = 0; i < userDetails.Count; i++)
                {
                    var user = userDetails[i];
                    var row = i + 2;

                    worksheet.Cells[row, 1].Value = user.Id;
                    worksheet.Cells[row, 2].Value = user.FirstName;
                    worksheet.Cells[row, 3].Value = user.LastName;
                    worksheet.Cells[row, 4].Value = user.Email;
                    worksheet.Cells[row, 5].Value = user.Active ? "Active" : "Inactive";
                    worksheet.Cells[row, 6].Value = user.CountryName;
                    worksheet.Cells[row, 7].Value = string.Join(", ", user.Roles);
                    worksheet.Cells[row, 8].Value = user.CreatedBy;
                    worksheet.Cells[row, 9].Value = user.CreatedOn?.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 10].Value = user.ModifiedBy;
                    worksheet.Cells[row, 11].Value = user.ModifiedOn?.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 12].Value = user.Gender;
                    worksheet.Cells[row, 13].Value = user.DateOfBirth?.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 14].Value = user.ProfileImageUrl;
                    worksheet.Cells[row, 15].Value = user.PhoneNumber;

                    // Company Details
                    worksheet.Cells[row, 16].Value = string.Join("; ",
                        user.AssignedCompanies.Select(c =>
                            $"{c.CompanyName} ({string.Join(", ", c.SellingChannels)})"));
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Return file
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Users_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

    }
}
