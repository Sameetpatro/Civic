namespace CivicFix.Application.Features.Departments;

public record DepartmentDto(
    Guid Id,
    string Name,
    string Code,
    string Description,
    string ContactEmail,
    string ContactPhone,
    string HeadOfficerName,
    int TotalActiveIssues,
    int TotalAvailableWorkers
);

public record CategoryDto(
    Guid Id,
    string Name,
    string Code,
    string PrimaryCategoryGroup,
    string Description,
    Guid DepartmentId,
    string DepartmentName,
    int DefaultSlaHours
);

public record WorkerSummaryDto(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    string Specialization,
    string AssignedWardOrZone,
    int ActiveJobsCount,
    int MaxCapacity,
    bool IsAvailable,
    double Rating,
    int TotalCompletedJobs
);
