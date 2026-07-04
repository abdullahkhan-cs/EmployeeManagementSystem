using System.ComponentModel.DataAnnotations;

namespace EMS.Web.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string Code { get; set; } = string.Empty;

        public ICollection<Designation> Designations { get; set; } = new List<Designation>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}