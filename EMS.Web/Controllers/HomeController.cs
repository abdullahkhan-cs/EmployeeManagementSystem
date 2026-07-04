using System.Diagnostics;
using EMS.Web.Data;
using EMS.Web.Models;
using EMS.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMS.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var employees = _context.Employees.AsQueryable();

            var model = new DashboardViewModel
            {
                TotalEmployees = await employees.CountAsync(),
                TotalDepartments = await _context.Departments.CountAsync(),
                ActiveCount = await employees.CountAsync(e => e.Status == EmployeeStatus.Active),
                OnLeaveCount = await employees.CountAsync(e => e.Status == EmployeeStatus.OnLeave),
                TerminatedCount = await employees.CountAsync(e => e.Status == EmployeeStatus.Terminated)
            };

            var deptCounts = await _context.Departments
                .Select(d => new { d.Name, Count = d.Employees.Count })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            model.DepartmentLabels = deptCounts.Select(x => x.Name).ToList();
            model.DepartmentCounts = deptCounts.Select(x => x.Count).ToList();

            model.RecentJoiners = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .OrderByDescending(e => e.JoiningDate)
                .Take(5)
                .Select(e => new RecentJoinerViewModel
                {
                    FullName = e.FirstName + " " + e.LastName,
                    DepartmentName = e.Department!.Name,
                    DesignationName = e.Designation!.Title,
                    JoiningDate = e.JoiningDate,
                    PhotoPath = e.ProfilePhotoPath
                })
                .ToListAsync();

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}