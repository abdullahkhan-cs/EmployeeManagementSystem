using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMS.Web.Models
{
    public enum EmployeeStatus
    {
        Active,
        OnLeave,
        Terminated
    }

    public class Employee
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Phone, StringLength(20)]
        public string? Phone { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public Department? Department { get; set; }

        [Required]
        public int DesignationId { get; set; }

        [ForeignKey(nameof(DesignationId))]
        public Designation? Designation { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Salary { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime JoiningDate { get; set; }

        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        [StringLength(300)]
        public string? ProfilePhotoPath { get; set; }

        // Link to Identity user (nullable — not every employee needs login access)
        public string? AppUserId { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}