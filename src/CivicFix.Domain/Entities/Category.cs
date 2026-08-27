namespace CivicFix.Domain.Entities;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // e.g. "PIPE_LEAK", "POTHOLE", "GARBAGE_DUMP"
    public string Description { get; set; } = string.Empty;
    public string PrimaryCategoryGroup { get; set; } = string.Empty; // e.g. "WATER", "ROADS", "GARBAGE"

    public Guid DepartmentId { get; set; }
    public virtual Department Department { get; set; } = null!;

    public int DefaultSlaHours { get; set; } = 48;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();
}
