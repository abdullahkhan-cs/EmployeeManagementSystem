using System.ComponentModel.DataAnnotations;

namespace EMS.Web.Models.ViewModels
{
    public class DepartmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100)]
        [Display(Name = "Department Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department code is required")]
        [StringLength(10)]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        public int EmployeeCount { get; set; }
    }
}