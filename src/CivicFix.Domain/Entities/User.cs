using CivicFix.Domain.Enums;

namespace CivicFix.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Citizen;
    
    // Optional department affiliation (for Department Officers and Field Workers)
    public Guid? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }

    public virtual WorkerProfile? WorkerProfile { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation collections
    public virtual ICollection<Issue> ReportedIssues { get; set; } = new List<Issue>();
    public virtual ICollection<Issue> AssignedIssues { get; set; } = new List<Issue>();
    public virtual ICollection<IssueStatusHistory> StatusChangesPerformed { get; set; } = new List<IssueStatusHistory>();
}
