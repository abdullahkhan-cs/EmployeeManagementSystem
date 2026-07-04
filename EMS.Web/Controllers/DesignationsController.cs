using EMS.Web.Data;
using EMS.Web.Models;
using EMS.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EMS.Web.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class DesignationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DesignationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Designations
        public async Task<IActionResult> Index()
        {
            var designations = await _context.Designations
                .Include(d => d.Department)
                .Select(d => new DesignationViewModel
                {
                    Id = d.Id,
                    Title = d.Title,
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.Department!.Name,
                    EmployeeCount = d.Employees.Count
                })
                .OrderBy(d => d.Title)
                .ToListAsync();

            return View(designations);
        }

        // GET: Designations/Create
        public async Task<IActionResult> Create()
        {
            var model = new DesignationViewModel
            {
                Departments = await GetDepartmentSelectList()
            };
            return View(model);
        }

        // POST: Designations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DesignationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = await GetDepartmentSelectList();
                return View(model);
            }

            var designation = new Designation
            {
                Title = model.Title,
                DepartmentId = model.DepartmentId
            };

            _context.Designations.Add(designation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Designation created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Designations/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var designation = await _context.Designations.FindAsync(id);
            if (designation == null) return NotFound();

            var model = new DesignationViewModel
            {
                Id = designation.Id,
                Title = designation.Title,
                DepartmentId = designation.DepartmentId,
                Departments = await GetDepartmentSelectList()
            };

            return View(model);
        }

        // POST: Designations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DesignationViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                model.Departments = await GetDepartmentSelectList();
                return View(model);
            }

            var designation = await _context.Designations.FindAsync(id);
            if (designation == null) return NotFound();

            designation.Title = model.Title;
            designation.DepartmentId = model.DepartmentId;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Designation updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Designations/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var designation = await _context.Designations
                .Include(d => d.Department)
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (designation == null) return NotFound();

            var model = new DesignationViewModel
            {
                Id = designation.Id,
                Title = designation.Title,
                DepartmentName = designation.Department!.Name,
                EmployeeCount = designation.Employees.Count
            };

            return View(model);
        }

        // POST: Designations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var designation = await _context.Designations
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (designation == null) return NotFound();

            if (designation.Employees.Any())
            {
                TempData["Error"] = "Cannot delete a designation that has employees assigned.";
                return RedirectToAction(nameof(Index));
            }

            _context.Designations.Remove(designation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Designation deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SelectListItem>> GetDepartmentSelectList()
        {
            return await _context.Departments
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                })
                .ToListAsync();
        }
    }
}