namespace EMS.Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public int ActiveCount { get; set; }
        public int OnLeaveCount { get; set; }
        public int TerminatedCount { get; set; }

        public List<string> DepartmentLabels { get; set; } = new();
        public List<int> DepartmentCounts { get; set; } = new();

        public List<RecentJoinerViewModel> RecentJoiners { get; set; } = new();
    }

    public class RecentJoinerViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public DateTime JoiningDate { get; set; }
        public string? PhotoPath { get; set; }
    }
}