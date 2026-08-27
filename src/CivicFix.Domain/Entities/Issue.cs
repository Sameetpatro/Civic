using CivicFix.Domain.Enums;

namespace CivicFix.Domain.Entities;

public class Issue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ReferenceNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // AI-extracted / enriched metadata
    public string? AiSummary { get; set; }
    public string? AiExtractedEntitiesJson { get; set; }
    public double? AiConfidenceScore { get; set; }

    // Relationships
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;

    public Guid? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }

    public Guid ReportedByUserId { get; set; }
    public virtual User ReportedByUser { get; set; } = null!;

    public Guid? AssignedWorkerId { get; set; }
    public virtual User? AssignedWorker { get; set; }

    // Status & Priorities
    public IssueStatus Status { get; set; } = IssueStatus.Reported;
    public IssueSeverity Severity { get; set; } = IssueSeverity.Medium;
    public IssuePriority Priority { get; set; } = IssuePriority.Medium;

    // Geospatial & Address Information (Sonipat coordinates focus)
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public string WardSector { get; set; } = string.Empty; // e.g. "Sector 14", "Model Town", "Murthal Road"

    // Timestamps and SLA
    public DateTime ReportedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? TargetSlaUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }

    // Special flags & Resolution metadata
    public bool IsSensitive { get; set; } = false; // Criminal, emergency or harassment flags
    public string? ResolutionNotes { get; set; }
    public string? CitizenFeedback { get; set; }
    public int? CitizenRating { get; set; } // 1 to 5

    // Duplicate detection / Master incident linkage
    public Guid? MasterIssueId { get; set; }
    public virtual Issue? MasterIssue { get; set; }
    public virtual ICollection<Issue> DuplicateIssues { get; set; } = new List<Issue>();

    // Navigation collections
    public virtual ICollection<IssuePhoto> Photos { get; set; } = new List<IssuePhoto>();
    public virtual ICollection<IssueStatusHistory> StatusHistory { get; set; } = new List<IssueStatusHistory>();

    /// <summary>
    /// Validates and records a state transition in the issue lifecycle.
    /// </summary>
    public IssueStatusHistory ChangeStatus(IssueStatus newStatus, Guid changedByUserId, string remarks, string? metadataJson = null)
    {
        var oldStatus = Status;
        Status = newStatus;
        UpdatedAtUtc = DateTime.UtcNow;

        if (newStatus == IssueStatus.Resolved)
        {
            ResolvedAtUtc = DateTime.UtcNow;
        }
        else if (newStatus == IssueStatus.Closed)
        {
            ClosedAtUtc = DateTime.UtcNow;
        }

        var historyRecord = new IssueStatusHistory
        {
            IssueId = Id,
            FromStatus = oldStatus,
            ToStatus = newStatus,
            ChangedByUserId = changedByUserId,
            ChangedAtUtc = DateTime.UtcNow,
            Remarks = remarks,
            MetadataJson = metadataJson
        };

        StatusHistory.Add(historyRecord);
        return historyRecord;
    }
}
