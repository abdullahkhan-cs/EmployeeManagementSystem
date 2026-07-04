using EMS.Web.Data;
using EMS.Web.Models;
using EMS.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace EMS.Web.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeesController(ApplicationDbContext context, IWebHostEnvironment env, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        // GET: Employees  (all logged-in roles can view)
        public async Task<IActionResult> Index(string? search, int? departmentId, EmployeeStatus? status)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    e.FirstName.Contains(search) ||
                    e.LastName.Contains(search) ||
                    e.Email.Contains(search));
            }

            if (departmentId.HasValue)
                query = query.Where(e => e.DepartmentId == departmentId.Value);

            if (status.HasValue)
                query = query.Where(e => e.Status == status.Value);

            var employees = await query
                .OrderBy(e => e.FirstName)
                .Select(e => new EmployeeViewModel
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    Phone = e.Phone,
                    Salary = e.Salary,
                    JoiningDate = e.JoiningDate,
                    Status = e.Status,
                    ExistingPhotoPath = e.ProfilePhotoPath,
                    DepartmentName = e.Department!.Name,
                    DesignationName = e.Designation!.Title,
                    HasLoginAccount = e.AppUserId != null
                })
                .ToListAsync();

            ViewBag.Departments = new SelectList(await _context.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", departmentId);
            ViewBag.SearchTerm = search;
            ViewBag.SelectedStatus = status;

            return View(employees);
        }

        // GET: Employees/MyProfile  (Employee role — view their own linked record)
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MyProfile()
        {
            var userId = _userManager.GetUserId(User);

            var e = await _context.Employees
                .Include(x => x.Department)
                .Include(x => x.Designation)
                .FirstOrDefaultAsync(x => x.AppUserId == userId);

            if (e == null)
            {
                TempData["Error"] = "No employee profile is linked to your account yet. Contact HR.";
                return RedirectToAction("Index", "Home");
            }

            var model = MapToViewModel(e);
            return View("Details", model);
        }

        // GET: Employees/Details/5  (Admin/HR only — Employee role uses MyProfile instead)
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Details(int id)
        {
            var e = await _context.Employees
                .Include(x => x.Department)
                .Include(x => x.Designation)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (e == null) return NotFound();

            var model = MapToViewModel(e);
            return View(model);
        }

        // GET: Employees/Create
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Create()
        {
            var model = new EmployeeViewModel
            {
                Departments = await GetDepartmentSelectList(),
                Designations = new List<SelectListItem>()
            };
            return View(model);
        }

        // POST: Employees/Create
        [Authorize(Roles = "Admin,HR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await RepopulateDropdowns(model);
                return View(model);
            }

            var employee = new Employee
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                DateOfBirth = model.DateOfBirth,
                Address = model.Address,
                DepartmentId = model.DepartmentId,
                DesignationId = model.DesignationId,
                Salary = model.Salary,
                JoiningDate = model.JoiningDate,
                Status = model.Status
            };

            if (model.PhotoFile != null)
                employee.ProfilePhotoPath = await SavePhotoAsync(model.PhotoFile);

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            string successMessage = "Employee added successfully.";

            if (model.CreateLoginAccount)
            {
                successMessage += await CreateLoginForEmployeeAsync(employee);
            }

            TempData["Success"] = successMessage;
            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Edit/5
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            var model = MapToViewModel(employee);
            await RepopulateDropdowns(model);
            return View(model);
        }

        // POST: Employees/Edit/5
        [Authorize(Roles = "Admin,HR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await RepopulateDropdowns(model);
                return View(model);
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            employee.FirstName = model.FirstName;
            employee.LastName = model.LastName;
            employee.Email = model.Email;
            employee.Phone = model.Phone;
            employee.DateOfBirth = model.DateOfBirth;
            employee.Address = model.Address;
            employee.DepartmentId = model.DepartmentId;
            employee.DesignationId = model.DesignationId;
            employee.Salary = model.Salary;
            employee.JoiningDate = model.JoiningDate;
            employee.Status = model.Status;

            if (model.PhotoFile != null)
                employee.ProfilePhotoPath = await SavePhotoAsync(model.PhotoFile);

            await _context.SaveChangesAsync();

            string successMessage = "Employee updated successfully.";

            if (model.CreateLoginAccount && employee.AppUserId == null)
            {
                successMessage += await CreateLoginForEmployeeAsync(employee);
            }

            TempData["Success"] = successMessage;
            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Delete/5
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            return View(MapToViewModel(employee));
        }

        // POST: Employees/Delete/5
        [Authorize(Roles = "Admin,HR")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            // If a login account is linked, remove it too
            if (!string.IsNullOrEmpty(employee.AppUserId))
            {
                var linkedUser = await _userManager.FindByIdAsync(employee.AppUserId);
                if (linkedUser != null)
                    await _userManager.DeleteAsync(linkedUser);
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Employee deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Employees/GetDesignationsByDepartment/5  (all logged-in roles can use — needed for Create/Edit forms)
        [HttpGet]
        public async Task<JsonResult> GetDesignationsByDepartment(int departmentId)
        {
            var designations = await _context.Designations
                .Where(d => d.DepartmentId == departmentId)
                .OrderBy(d => d.Title)
                .Select(d => new { d.Id, d.Title })
                .ToListAsync();

            return Json(designations);
        }

        // GET: Employees/ExportExcel
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> ExportExcel(string? search, int? departmentId, EmployeeStatus? status)
        {
            var employees = await GetFilteredEmployeesForExport(search, departmentId, status);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var sheet = workbook.Worksheets.Add("Employees");

            string[] headers = { "First Name", "Last Name", "Email", "Phone", "Department", "Designation", "Salary", "Joining Date", "Status" };
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cell(1, i + 1).Value = headers[i];
                sheet.Cell(1, i + 1).Style.Font.Bold = true;
                sheet.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#0f1b3c");
                sheet.Cell(1, i + 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }

            int row = 2;
            foreach (var e in employees)
            {
                sheet.Cell(row, 1).Value = e.FirstName;
                sheet.Cell(row, 2).Value = e.LastName;
                sheet.Cell(row, 3).Value = e.Email;
                sheet.Cell(row, 4).Value = e.Phone;
                sheet.Cell(row, 5).Value = e.DepartmentName;
                sheet.Cell(row, 6).Value = e.DesignationName;
                sheet.Cell(row, 7).Value = e.Salary;
                sheet.Cell(row, 8).Value = e.JoiningDate.ToString("dd MMM yyyy");
                sheet.Cell(row, 9).Value = e.Status.ToString();
                row++;
            }

            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Employees_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // GET: Employees/ExportPdf
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> ExportPdf(string? search, int? departmentId, EmployeeStatus? status)
        {
            var employees = await GetFilteredEmployeesForExport(search, departmentId, status);

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    //page.Size(QuestPDF.Helpers.PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Text("Employee Report").FontSize(18).Bold().FontColor(QuestPDF.Helpers.Colors.Blue.Darken3);

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            string[] headers = { "Name", "Email", "Department", "Designation", "Salary", "Joined", "Status" };
                            foreach (var h in headers)
                            {
                                header.Cell().Background(QuestPDF.Helpers.Colors.Blue.Darken3).Padding(5)
                                    .Text(h).FontColor(QuestPDF.Helpers.Colors.White).Bold();
                            }
                        });

                        foreach (var e in employees)
                        {
                            table.Cell().Padding(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Text($"{e.FirstName} {e.LastName}");
                            table.Cell().Padding(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Text(e.Email);
                            table.Cell().Padding(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Text(e.DepartmentName);
                            table.Cell().Padding(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Text(e.DesignationName);
                            table.Cell().Padding(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Text(e.Salary.ToString("N0"));
                            table.Cell().Padding(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Text(e.JoiningDate.ToString("dd MMM yyyy"));
                            table.Cell().Padding(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Text(e.Status.ToString());
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated on ");
                        x.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm"));
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Employees_{DateTime.Now:yyyyMMdd}.pdf");
        }

        // ----- Helpers -----

        private EmployeeViewModel MapToViewModel(Employee e)
        {
            return new EmployeeViewModel
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Phone = e.Phone,
                DateOfBirth = e.DateOfBirth,
                Address = e.Address,
                DepartmentId = e.DepartmentId,
                DesignationId = e.DesignationId,
                Salary = e.Salary,
                JoiningDate = e.JoiningDate,
                Status = e.Status,
                ExistingPhotoPath = e.ProfilePhotoPath,
                DepartmentName = e.Department?.Name,
                DesignationName = e.Designation?.Title,
                HasLoginAccount = e.AppUserId != null
            };
        }

        private async Task RepopulateDropdowns(EmployeeViewModel model)
        {
            model.Departments = await GetDepartmentSelectList();
            model.Designations = await _context.Designations
                .Where(d => d.DepartmentId == model.DepartmentId)
                .OrderBy(d => d.Title)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Title })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetDepartmentSelectList()
        {
            return await _context.Departments
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
                .ToListAsync();
        }

        private async Task<string> SavePhotoAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "employees");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/employees/{fileName}";
        }

        // Creates an Identity login for an employee, assigns "Employee" role, links both ways.
        // Returns a message fragment with the temp password to show the admin/HR once.
        private async Task<string> CreateLoginForEmployeeAsync(Employee employee)
        {
            var existingUser = await _userManager.FindByEmailAsync(employee.Email);
            if (existingUser != null)
                return " (Note: a login account with this email already exists — link it manually if needed.)";

            var tempPassword = $"Emp@{Guid.NewGuid().ToString("N").Substring(0, 6)}";

            var appUser = new ApplicationUser
            {
                UserName = employee.Email,
                Email = employee.Email,
                EmailConfirmed = true,
                EmployeeId = employee.Id
            };

            var result = await _userManager.CreateAsync(appUser, tempPassword);
            if (!result.Succeeded)
                return " (Login account creation failed — you can retry from Edit.)";

            await _userManager.AddToRoleAsync(appUser, "Employee");

            employee.AppUserId = appUser.Id;
            await _context.SaveChangesAsync();

            return $" Login created — Email: {employee.Email}, Temp Password: {tempPassword} (share this with the employee).";
        }

        private async Task<List<EmployeeViewModel>> GetFilteredEmployeesForExport(string? search, int? departmentId, EmployeeStatus? status)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    e.FirstName.Contains(search) ||
                    e.LastName.Contains(search) ||
                    e.Email.Contains(search));
            }

            if (departmentId.HasValue)
                query = query.Where(e => e.DepartmentId == departmentId.Value);

            if (status.HasValue)
                query = query.Where(e => e.Status == status.Value);

            return await query
                .OrderBy(e => e.FirstName)
                .Select(e => new EmployeeViewModel
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    Phone = e.Phone,
                    Salary = e.Salary,
                    JoiningDate = e.JoiningDate,
                    Status = e.Status,
                    DepartmentName = e.Department!.Name,
                    DesignationName = e.Designation!.Title
                })
                .ToListAsync();
        }
    }
}