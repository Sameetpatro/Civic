using CivicFix.Domain.Enums;

namespace CivicFix.Domain.Entities;

public class IssuePhoto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid IssueId { get; set; }
    public virtual Issue Issue { get; set; } = null!;

    public string PhotoUrl { get; set; } = string.Empty;
    public PhotoType PhotoType { get; set; } = PhotoType.ReportBefore;
    public string? Caption { get; set; }

    public Guid UploadedByUserId { get; set; }
    public virtual User UploadedByUser { get; set; } = null!;

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}
