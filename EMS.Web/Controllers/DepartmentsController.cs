using EMS.Web.Data;
using EMS.Web.Models;
using EMS.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMS.Web.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                .Select(d => new DepartmentViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Code = d.Code,
                    EmployeeCount = d.Employees.Count
                })
                .OrderBy(d => d.Name)
                .ToListAsync();

            return View(departments);
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            return View(new DepartmentViewModel());
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var department = new Department
            {
                Name = model.Name,
                Code = model.Code
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Department created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            var model = new DepartmentViewModel
            {
                Id = department.Id,
                Name = department.Name,
                Code = department.Code
            };

            return View(model);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentViewModel model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            department.Name = model.Name;
            department.Code = model.Code;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Department updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null) return NotFound();

            var model = new DepartmentViewModel
            {
                Id = department.Id,
                Name = department.Name,
                Code = department.Code,
                EmployeeCount = department.Employees.Count
            };

            return View(model);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null) return NotFound();

            if (department.Employees.Any())
            {
                TempData["Error"] = "Cannot delete a department that has employees assigned.";
                return RedirectToAction(nameof(Index));
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Department deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}