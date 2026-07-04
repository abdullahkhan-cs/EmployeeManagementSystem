using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EMS.Web.Models.ViewModels
{
    public class DesignationViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a department")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        public string? DepartmentName { get; set; }

        public int EmployeeCount { get; set; }

        public List<SelectListItem>? Departments { get; set; }
    }
}