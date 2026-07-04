using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using EMS.Web.Models;

namespace EMS.Web.Models.ViewModels
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Please select a department")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Please select a designation")]
        [Display(Name = "Designation")]
        public int DesignationId { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        [Range(0, 100000000, ErrorMessage = "Enter a valid salary")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Joining date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Joining Date")]
        public DateTime JoiningDate { get; set; } = DateTime.Today;

        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        public string? ExistingPhotoPath { get; set; }

        [Display(Name = "Profile Photo")]
        public IFormFile? PhotoFile { get; set; }

        // Self-service login fields
        public bool HasLoginAccount { get; set; }

        [Display(Name = "Create login account for this employee")]
        public bool CreateLoginAccount { get; set; }

        // Read-only display fields (used in Index/Details)
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        // Dropdown sources
        public List<SelectListItem>? Departments { get; set; }
        public List<SelectListItem>? Designations { get; set; }
    }
}