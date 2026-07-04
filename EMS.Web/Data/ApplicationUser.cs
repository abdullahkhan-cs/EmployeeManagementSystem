using Microsoft.AspNetCore.Identity;

namespace EMS.Web.Data
{
    public class ApplicationUser : IdentityUser
    {
        public int? EmployeeId { get; set; }
    }
}