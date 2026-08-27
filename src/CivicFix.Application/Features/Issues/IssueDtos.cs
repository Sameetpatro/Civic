using CivicFix.Domain.Enums;

namespace CivicFix.Application.Features.Issues;

public record CreateIssueRequestDto(
    string Title,
    string Description,
    Guid CategoryId,
    double Latitude,
    double Longitude,
    string Address,
    string WardSector,
    bool IsSensitive = false,
    List<string>? PhotoUrls = null
);

public record IssueResponseDto(
    Guid Id,
    string ReferenceNumber,
    string Title,
    string Description,
    string? AiSummary,
    Guid CategoryId,
    string CategoryName,
    string PrimaryCategoryGroup,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid ReportedByUserId,
    string ReportedByUserName,
    Guid? AssignedWorkerId,
    string? AssignedWorkerName,
    IssueStatus Status,
    string StatusName,
    IssueSeverity Severity,
    string SeverityName,
    IssuePriority Priority,
    string PriorityName,
    double Latitude,
    double Longitude,
    string Address,
    string WardSector,
    DateTime ReportedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? TargetSlaUtc,
    DateTime? ResolvedAtUtc,
    DateTime? ClosedAtUtc,
    bool IsSensitive,
    string? ResolutionNotes,
    string? CitizenFeedback,
    int? CitizenRating,
    Guid? MasterIssueId,
    List<IssuePhotoDto> Photos,
    List<IssueStatusHistoryDto> StatusHistory
);

public record IssueSummaryDto(
    Guid Id,
    string ReferenceNumber,
    string Title,
    string CategoryName,
    string PrimaryCategoryGroup,
    string? DepartmentName,
    IssueStatus Status,
    string StatusName,
    IssueSeverity Severity,
    string SeverityName,
    IssuePriority Priority,
    string PriorityName,
    double Latitude,
    double Longitude,
    string WardSector,
    DateTime ReportedAtUtc,
    DateTime? TargetSlaUtc,
    string? ThumbnailUrl,
    bool IsSlaBreached
);

public record IssueFilterDto(
    Guid? DepartmentId = null,
    IssueStatus? Status = null,
    IssueSeverity? Severity = null,
    string? WardSector = null,
    Guid? CategoryId = null,
    Guid? ReportedByUserId = null,
    Guid? AssignedWorkerId = null,
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 20
);

public record PagedResultDto<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

public record AssignDepartmentRequestDto(
    Guid DepartmentId,
    string Remarks
);

public record AssignWorkerRequestDto(
    Guid WorkerId,
    string Remarks
);

public record ResolveIssueRequestDto(
    string ResolutionNotes,
    List<string>? AfterPhotoUrls = null
);

public record VerifyIssueRequestDto(
    bool IsSatisfied,
    string? Feedback,
    int? Rating = 5
);

public record ChangeStatusRequestDto(
    IssueStatus NewStatus,
    string Remarks,
    string? MetadataJson = null
);

public record MarkDuplicateRequestDto(
    Guid MasterIssueId,
    string Remarks
);

public record IssuePhotoDto(
    Guid Id,
    string PhotoUrl,
    PhotoType PhotoType,
    string PhotoTypeName,
    string? Caption,
    Guid UploadedByUserId,
    DateTime UploadedAtUtc
);

public record IssueStatusHistoryDto(
    Guid Id,
    IssueStatus FromStatus,
    string FromStatusName,
    IssueStatus ToStatus,
    string ToStatusName,
    Guid ChangedByUserId,
    string ChangedByUserName,
    DateTime ChangedAtUtc,
    string Remarks
);

public record NearbyIssuesRequestDto(
    double Latitude,
    double Longitude,
    double RadiusKm = 2.0
);
