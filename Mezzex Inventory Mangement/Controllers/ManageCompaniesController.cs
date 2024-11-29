using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Mezzex_Inventory_Mangement.Models;
using Mezzex_Inventory_Mangement.Services;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using static Mezzex_Inventory_Mangement.Controllers.UsersController;

namespace Mezzex_Inventory_Mangement.Controllers
{
    public class ManageCompaniesController : Controller
    {
        private readonly ManageCompanyService _companyService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManageCompaniesController(ManageCompanyService companyService, UserManager<ApplicationUser> userManager)
        {
            _companyService = companyService;
            _userManager = userManager;
        }

        // GET: ManageCompanies
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (User.IsInRole("Administrator") || User.IsInRole("Admin"))
            {
                var companies = await _companyService.GetAllCompaniesAsync();
                return View(companies);
            }
            else
            {
                var companies = await _companyService.GetAllCompaniesAsync();
                return View(companies);
            }
        }

        // GET: ManageCompanies/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null)
            {
                TempData["ErrorMessage"] = "Company not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: ManageCompanies/Create
        public IActionResult Create()
        {
            ViewBag.CompanyStatusOptions = new SelectList(ManageCompany.CompanyStatusOptions);
            ViewBag.YearStartMonthOptions = new SelectList(ManageCompany.YearStartMonthOptions);
            ViewBag.VatCycleOptions = new SelectList(ManageCompany.VatCycleOptions);
            return View();
        }

        // POST: ManageCompanies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ManageCompany manageCompany, IFormFile companyLogo)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                manageCompany.CreatedBy = $"{user.FirstName} {user.LastName}";
            }
            manageCompany.CreatedOn = DateTime.Now;

            if (companyLogo != null && companyLogo.Length > 0)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://smapi.mezzex.com/api/UploadData/");
                    var formData = new MultipartFormDataContent();

                    var fileContent = new StreamContent(companyLogo.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(companyLogo.ContentType);
                    formData.Add(fileContent, "file", companyLogo.FileName);

                    var response = await client.PostAsync("upload-Images", formData);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<UploadResponse>();
                        if (result != null && !string.IsNullOrEmpty(result.FileName))
                        {
                            manageCompany.CompanyLogoUrl = $"https://sm.mezzex.com/ScreenShot/{result.FileName}";
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to upload the company logo. Please try again.");
                            ViewBag.CompanyStatusOptions = new SelectList(ManageCompany.CompanyStatusOptions);
                            ViewBag.YearStartMonthOptions = new SelectList(ManageCompany.YearStartMonthOptions);
                            ViewBag.VatCycleOptions = new SelectList(ManageCompany.VatCycleOptions);
                            return View(manageCompany);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error while uploading the company logo to the server.");
                        ViewBag.CompanyStatusOptions = new SelectList(ManageCompany.CompanyStatusOptions);
                        ViewBag.YearStartMonthOptions = new SelectList(ManageCompany.YearStartMonthOptions);
                        ViewBag.VatCycleOptions = new SelectList(ManageCompany.VatCycleOptions);
                        return View(manageCompany);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                if (await _companyService.CreateCompanyAsync(manageCompany))
                {
                    TempData["SuccessMessage"] = "Company created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "Error occurred while creating the company.";
            }

            ViewBag.CompanyStatusOptions = new SelectList(ManageCompany.CompanyStatusOptions);
            ViewBag.YearStartMonthOptions = new SelectList(ManageCompany.YearStartMonthOptions);
            ViewBag.VatCycleOptions = new SelectList(ManageCompany.VatCycleOptions);
            return View(manageCompany);
        }

        // GET: ManageCompanies/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null)
            {
                TempData["ErrorMessage"] = "Company not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CompanyStatusOptions = new SelectList(ManageCompany.CompanyStatusOptions);
            ViewBag.YearStartMonthOptions = new SelectList(ManageCompany.YearStartMonthOptions);
            ViewBag.VatCycleOptions = new SelectList(ManageCompany.VatCycleOptions);

            return View(company);
        }

        // POST: ManageCompanies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ManageCompany manageCompany, IFormFile companyLogo)
        {
            if (id != manageCompany.CompanyId)
            {
                TempData["ErrorMessage"] = "Invalid company ID.";
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                manageCompany.ModifiedBy = $"{user.FirstName} {user.LastName}";
            }
            manageCompany.ModifiedOn = DateTime.Now;

            if (companyLogo != null && companyLogo.Length > 0)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://smapi.mezzex.com/api/UploadData/");
                    var formData = new MultipartFormDataContent();

                    var fileContent = new StreamContent(companyLogo.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(companyLogo.ContentType);
                    formData.Add(fileContent, "file", companyLogo.FileName);

                    var response = await client.PostAsync("upload-Images", formData);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<UploadResponse>();
                        if (result != null && !string.IsNullOrEmpty(result.FileName))
                        {
                            manageCompany.CompanyLogoUrl = $"https://sm.mezzex.com/ScreenShot/{result.FileName}";
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to upload the company logo. Please try again.");
                            return View(manageCompany);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error while uploading the company logo to the server.");
                        return View(manageCompany);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                if (await _companyService.UpdateCompanyAsync(manageCompany))
                {
                    TempData["SuccessMessage"] = "Company updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "Error occurred while updating the company.";
            }

            return View(manageCompany);
        }
        // GET: ManageCompanies/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null)
            {
                TempData["ErrorMessage"] = "Company not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await _companyService.DeleteCompanyAsync(id))
            {
                TempData["SuccessMessage"] = "Company deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error occurred while deleting the company.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ExportToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var companies = await _companyService.GetAllCompaniesAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Companies");

                // Adding header row (start from column 1)
                var headers = new string[]
                {
                    "CompanyName", "CountryName", "Email", "WebsiteName", "Phone",
                    "Address", "Postcode", "City", "State", "Fax", "VatNumber",
                    "CompanyNumber", "CurrencyCode", "InvoicePrefixNumber", "CompanyStatus",
                    "YearStartMonth", "VatCycle", "BankName", "AccountName", "AccountNumber",
                    "IFSCCode", "AccountType", "SortCode", "IBAN", "BIC", "BankAddress"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }

                // Style for header
                using (var range = worksheet.Cells[1, 1, 1, headers.Length])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Populate data rows
                int row = 2;
                foreach (var company in companies)
                {
                    worksheet.Cells[row, 1].Value = company.CompanyName;
                    worksheet.Cells[row, 2].Value = company.CountryName;
                    worksheet.Cells[row, 3].Value = company.Email;
                    worksheet.Cells[row, 4].Value = company.WebsiteName;
                    worksheet.Cells[row, 5].Value = company.Phone;
                    worksheet.Cells[row, 6].Value = company.Address;
                    worksheet.Cells[row, 7].Value = company.Postcode;
                    worksheet.Cells[row, 8].Value = company.City;
                    worksheet.Cells[row, 9].Value = company.State;
                    worksheet.Cells[row, 10].Value = company.Fax;
                    worksheet.Cells[row, 11].Value = company.VatNumber;
                    worksheet.Cells[row, 12].Value = company.CompanyNumber;
                    worksheet.Cells[row, 13].Value = company.CurrencyCode;
                    worksheet.Cells[row, 14].Value = company.InvoicePrefixNumber;
                    worksheet.Cells[row, 15].Value = company.CompanyStatus;
                    worksheet.Cells[row, 16].Value = company.YearStartMonth;
                    worksheet.Cells[row, 17].Value = company.VatCycle;
                    worksheet.Cells[row, 18].Value = company.BankName;
                    worksheet.Cells[row, 19].Value = company.AccountName;
                    worksheet.Cells[row, 20].Value = company.AccountNumber;
                    worksheet.Cells[row, 21].Value = company.IFSCCode;
                    worksheet.Cells[row, 22].Value = company.AccountType;
                    worksheet.Cells[row, 23].Value = company.SortCode;
                    worksheet.Cells[row, 24].Value = company.IBAN;
                    worksheet.Cells[row, 25].Value = company.BIC;
                    worksheet.Cells[row, 26].Value = company.BankAddress;
                    row++;
                }

                // Auto-fit columns to adjust based on content
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Convert the Excel package to a byte array
                var fileContent = package.GetAsByteArray();

                // Send the Excel file as a downloadable response
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Companies.xlsx");
            }
        }
    }
}
