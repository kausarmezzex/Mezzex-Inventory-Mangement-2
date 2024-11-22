using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Mezzex_Inventory_Mangement.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Mezzex_Inventory_Mangement.ViewModels;

namespace Mezzex_Inventory_Mangement.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser model, string password, List<string> selectedRoles, IFormFile profileImage)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserName = $"{currentUser.FirstName} {currentUser.LastName}";

            if (ModelState.IsValid)
            {
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
                    CreatedBy = currentUserName,
                    PhoneNumber = model.PhoneNumber,
                    DateOfBirth = model.DateOfBirth, // Add DateOfBirth here 
                    ProfileImageUrl = imageUrl // Save uploaded image URL
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
                    TempData["ErrorMessage"] = "Error occurred while creating the User.";
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Return the view with roles if something went wrong
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
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

            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            ViewBag.UserRoles = userRoles;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ApplicationUser model, List<string> selectedRoles, IFormFile profileImage)
        {
            ModelState.Remove("profileImage"); // Optional fields
            var user = await _userManager.FindByIdAsync(id.ToString());
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
                user.ModifiedBy = $"{User.Identity.Name}";
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
            var userPermissions = user.UserPermissions?.Select(p => p.Permission.Name).ToList();

            var userDetailsViewModel = new UserDetailsViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Active = user.Active,
                CountryName = user.CountryName,
                Roles = roles.ToList(),
                Permissions = userPermissions,
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


    }
}
