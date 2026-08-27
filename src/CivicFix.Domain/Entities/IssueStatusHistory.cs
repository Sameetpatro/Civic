using CivicFix.Domain.Enums;

namespace CivicFix.Domain.Entities;

public class IssueStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid IssueId { get; set; }
    public virtual Issue Issue { get; set; } = null!;

    public IssueStatus FromStatus { get; set; }
    public IssueStatus ToStatus { get; set; }

    public Guid ChangedByUserId { get; set; }
    public virtual User ChangedByUser { get; set; } = null!;

    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
    public string Remarks { get; set; } = string.Empty;
    public string? MetadataJson { get; set; }
}
