using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Mezzex_Inventory_Mangement.Models;
using Mezzex_Inventory_Mangement.ViewModels;
using Microsoft.EntityFrameworkCore;
using Mezzex_Inventory_Mangement.Services;
using Microsoft.AspNetCore.Authorization;
using Mezzex_Inventory_Mangement.Data;

namespace Mezzex_Inventory_Mangement.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _context;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, EmailService emailService, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailService = emailService;
            this._context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (model.Login.All(char.IsDigit))
            {
                model.PhoneNumber = model.Login;
            }

            if (ModelState.IsValid)
            {
                ApplicationUser user;

                if (model.Login == model.PhoneNumber)
                {
                    user = await _userManager.FindByNameAsync(model.Login)
                        ?? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
                }
                else
                {
                    user = await _userManager.FindByNameAsync(model.Login);
                }

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, false);

                    if (result.Succeeded)
                    {
                        // Check user assignments
                        var assignments = await _context.UserCompanyAssignments
                            .Where(a => a.UserId == user.Id)
                            .Include(a => a.Company)
                            .ToListAsync();

                        if (assignments.Count == 1)
                        {
                            // If one company is assigned, redirect directly
                            var singleCompany = assignments.First();
                            TempData["SelectedCompany"] = singleCompany.CompanyId;
                            HttpContext.Session.SetInt32("SelectedCompanyId", singleCompany.CompanyId); // Store in session
                            return RedirectToAction("Index", "Home");
                        }
                        else if (assignments.Count > 1)
                        {
                            // If multiple companies, redirect to selection page
                            return RedirectToAction("SelectCompany", "Account");
                        }

                        // If no assignments, log out and show error
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError("", "No company assigned to this user.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid login attempt. Please check your password.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "User not found. Please check your username or phone number.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Please fill in all required fields.");
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SelectCompany()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            // Fetch all company assignments for the current user
            var assignments = await _context.UserCompanyAssignments
                .Where(a => a.UserId == currentUser.Id)
                .Include(a => a.Company)
                .ToListAsync();

            // Read the selected company ID from the session
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId") ?? 0;


            // Prepare the ViewModel with the assigned companies and selected company
            var viewModel = new SelectCompanyViewModel
            {
                Companies = assignments.Select(a => a.Company).ToList(),
                SelectedCompanyId = selectedCompanyId // Use session value instead of TempData
            };

            return View(viewModel);
        }


        [HttpPost]
        [Authorize]
        public IActionResult SelectCompany(SelectCompanyViewModel model)
        {
            HttpContext.Session.SetInt32("SelectedCompanyId", model.SelectedCompanyId); // Store in session
            return RedirectToAction("Index", "Home");
        }



        // ForgotPassword Actions
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            // Generate OTP
            var otp = new Random().Next(100000, 999999).ToString();
            user.PasswordResetToken = otp;
            user.PasswordResetTokenExpiration = DateTime.UtcNow.AddMinutes(10);
            await _userManager.UpdateAsync(user);

            // Send OTP via email
            await _emailService.SendOtpEmailAsync(user.Email, otp);
            TempData["SuccessMessage"] = "OTP sent successfully to your email.";
            TempData["Email"] = model.Email;
            return RedirectToAction("VerifyOtp");
        }

        // VerifyOtp Actions
        [HttpGet]
        public IActionResult VerifyOtp() => View();

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.PasswordResetToken != model.Otp || user.PasswordResetTokenExpiration < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Invalid OTP or OTP has expired.");
                return View();
            }

            // Clear the OTP after successful verification
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiration = null;
            await _userManager.UpdateAsync(user);

            TempData["Email"] = model.Email;
            return RedirectToAction("ResetPassword");
        }

        // ResetPassword Actions
        [HttpGet]
        public IActionResult ResetPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View();
            }

            var result = await _userManager.ResetPasswordAsync(user, await _userManager.GeneratePasswordResetTokenAsync(user), model.NewPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password changed successfully. You can now log in with your new password.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View("AccessDenied"); // Updated view name
        }
    }
}
