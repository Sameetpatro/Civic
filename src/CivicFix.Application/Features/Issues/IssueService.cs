using CivicFix.Application.Common.Interfaces;
using CivicFix.Domain.Entities;
using CivicFix.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CivicFix.Application.Features.Issues;

public class IssueService : IIssueService
{
    private readonly IApplicationDbContext _context;
    private readonly IValidator<CreateIssueRequestDto> _createValidator;
    private readonly IValidator<ResolveIssueRequestDto> _resolveValidator;
    private readonly IValidator<VerifyIssueRequestDto> _verifyValidator;

    public IssueService(
        IApplicationDbContext context,
        IValidator<CreateIssueRequestDto> createValidator,
        IValidator<ResolveIssueRequestDto> resolveValidator,
        IValidator<VerifyIssueRequestDto> verifyValidator)
    {
        _context = context;
        _createValidator = createValidator;
        _resolveValidator = resolveValidator;
        _verifyValidator = verifyValidator;
    }

    public async Task<IssueResponseDto> CreateIssueAsync(CreateIssueRequestDto request, Guid reportingUserId, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var category = await _context.Categories
            .Include(c => c.Department)
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID '{request.CategoryId}' does not exist.");
        }

        var reporter = await _context.Users.FindAsync(new object[] { reportingUserId }, cancellationToken);
        if (reporter == null)
        {
            throw new KeyNotFoundException($"User with ID '{reportingUserId}' does not exist.");
        }

        var randomSuffix = RandomNumberGenerator.GetInt32(1000, 9999);
        var refNumber = $"CVX-{DateTime.UtcNow:yyyyMMdd}-{randomSuffix}";

        var issue = new Issue
        {
            ReferenceNumber = refNumber,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            CategoryId = category.Id,
            DepartmentId = category.DepartmentId,
            ReportedByUserId = reportingUserId,
            Status = IssueStatus.DepartmentAssigned, // Automatically mapped to the responsible municipal department
            Severity = IssueSeverity.Medium,
            Priority = IssuePriority.Medium,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Address = request.Address.Trim(),
            WardSector = request.WardSector.Trim(),
            ReportedAtUtc = DateTime.UtcNow,
            TargetSlaUtc = DateTime.UtcNow.AddHours(category.DefaultSlaHours),
            IsSensitive = request.IsSensitive
        };

        // Add photos if provided
        if (request.PhotoUrls != null && request.PhotoUrls.Any())
        {
            foreach (var photoUrl in request.PhotoUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                issue.Photos.Add(new IssuePhoto
                {
                    PhotoUrl = photoUrl,
                    PhotoType = PhotoType.ReportBefore,
                    UploadedByUserId = reportingUserId,
                    UploadedAtUtc = DateTime.UtcNow
                });
            }
        }

        // Add initial status audit trail
        issue.StatusHistory.Add(new IssueStatusHistory
        {
            FromStatus = IssueStatus.Reported,
            ToStatus = IssueStatus.DepartmentAssigned,
            ChangedByUserId = reportingUserId,
            ChangedAtUtc = DateTime.UtcNow,
            Remarks = $"Issue reported and automatically routed to {category.Department?.Name ?? "Department"} based on category '{category.Name}'."
        });

        await _context.Issues.AddAsync(issue, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetIssueByIdAsync(issue.Id, cancellationToken);
    }

    public async Task<IssueResponseDto> GetIssueByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var issue = await _context.Issues
            .Include(i => i.Category)
            .Include(i => i.Department)
            .Include(i => i.ReportedByUser)
            .Include(i => i.AssignedWorker)
            .Include(i => i.Photos)
            .Include(i => i.StatusHistory)
                .ThenInclude(h => h.ChangedByUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (issue == null)
        {
            throw new KeyNotFoundException($"Issue with ID '{id}' was not found.");
        }

        return MapToResponseDto(issue);
    }

    public async Task<PagedResultDto<IssueSummaryDto>> GetIssuesAsync(IssueFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Issues
            .Include(i => i.Category)
            .Include(i => i.Department)
            .Include(i => i.Photos)
            .AsNoTracking()
            .AsQueryable();

        if (filter.DepartmentId.HasValue)
        {
            query = query.Where(i => i.DepartmentId == filter.DepartmentId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(i => i.Status == filter.Status.Value);
        }

        if (filter.Severity.HasValue)
        {
            query = query.Where(i => i.Severity == filter.Severity.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == filter.CategoryId.Value);
        }

        if (filter.ReportedByUserId.HasValue)
        {
            query = query.Where(i => i.ReportedByUserId == filter.ReportedByUserId.Value);
        }

        if (filter.AssignedWorkerId.HasValue)
        {
            query = query.Where(i => i.AssignedWorkerId == filter.AssignedWorkerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.WardSector))
        {
            var sector = filter.WardSector.Trim().ToLower();
            query = query.Where(i => i.WardSector.ToLower().Contains(sector));
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim().ToLower();
            query = query.Where(i => i.Title.ToLower().Contains(search) 
                                  || i.Description.ToLower().Contains(search)
                                  || i.ReferenceNumber.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(i => i.ReportedAtUtc)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(i => new IssueSummaryDto(
                i.Id,
                i.ReferenceNumber,
                i.Title,
                i.Category.Name,
                i.Category.PrimaryCategoryGroup,
                i.Department != null ? i.Department.Name : null,
                i.Status,
                i.Status.ToString(),
                i.Severity,
                i.Severity.ToString(),
                i.Priority,
                i.Priority.ToString(),
                i.Latitude,
                i.Longitude,
                i.WardSector,
                i.ReportedAtUtc,
                i.TargetSlaUtc,
                i.Photos.OrderBy(p => p.UploadedAtUtc).Select(p => p.PhotoUrl).FirstOrDefault(),
                i.TargetSlaUtc.HasValue && DateTime.UtcNow > i.TargetSlaUtc.Value && i.Status != IssueStatus.Closed && i.Status != IssueStatus.Resolved
            ))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        return new PagedResultDto<IssueSummaryDto>(items, totalCount, filter.PageNumber, filter.PageSize, totalPages);
    }

    public async Task<List<IssueSummaryDto>> GetNearbyIssuesAsync(NearbyIssuesRequestDto request, CancellationToken cancellationToken = default)
    {
        var activeIssues = await _context.Issues
            .Include(i => i.Category)
            .Include(i => i.Department)
            .Include(i => i.Photos)
            .Where(i => i.Status != IssueStatus.Closed && i.Status != IssueStatus.Rejected && i.Status != IssueStatus.Duplicate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Filter by Haversine Distance (in-memory for maximum portability across DB engines)
        var nearby = activeIssues
            .Where(i => CalculateDistanceKm(request.Latitude, request.Longitude, i.Latitude, i.Longitude) <= request.RadiusKm)
            .Select(i => new IssueSummaryDto(
                i.Id,
                i.ReferenceNumber,
                i.Title,
                i.Category.Name,
                i.Category.PrimaryCategoryGroup,
                i.Department != null ? i.Department.Name : null,
                i.Status,
                i.Status.ToString(),
                i.Severity,
                i.Severity.ToString(),
                i.Priority,
                i.Priority.ToString(),
                i.Latitude,
                i.Longitude,
                i.WardSector,
                i.ReportedAtUtc,
                i.TargetSlaUtc,
                i.Photos.OrderBy(p => p.UploadedAtUtc).Select(p => p.PhotoUrl).FirstOrDefault(),
                i.TargetSlaUtc.HasValue && DateTime.UtcNow > i.TargetSlaUtc.Value
            ))
            .ToList();

        return nearby;
    }

    public async Task<IssueResponseDto> AssignDepartmentAsync(Guid issueId, AssignDepartmentRequestDto request, Guid officerUserId, CancellationToken cancellationToken = default)
    {
        var issue = await _context.Issues.Include(i => i.StatusHistory).FirstOrDefaultAsync(i => i.Id == issueId, cancellationToken);
        if (issue == null) throw new KeyNotFoundException($"Issue '{issueId}' not found.");

        var department = await _context.Departments.FindAsync(new object[] { request.DepartmentId }, cancellationToken);
        if (department == null) throw new KeyNotFoundException($"Department '{request.DepartmentId}' not found.");

        issue.DepartmentId = department.Id;
        issue.ChangeStatus(IssueStatus.DepartmentAssigned, officerUserId, $"Re-routed to department '{department.Name}'. Remarks: {request.Remarks}");

        await _context.SaveChangesAsync(cancellationToken);
        return await GetIssueByIdAsync(issueId, cancellationToken);
    }

    public async Task<IssueResponseDto> AssignWorkerAsync(Guid issueId, AssignWorkerRequestDto request, Guid officerUserId, CancellationToken cancellationToken = default)
    {
        var issue = await _context.Issues.Include(i => i.StatusHistory).FirstOrDefaultAsync(i => i.Id == issueId, cancellationToken);
        if (issue == null) throw new KeyNotFoundException($"Issue '{issueId}' not found.");

        var worker = await _context.Users
            .Include(u => u.WorkerProfile)
            .FirstOrDefaultAsync(u => u.Id == request.WorkerId && u.Role == UserRole.FieldWorker, cancellationToken);

        if (worker == null) throw new KeyNotFoundException($"Field worker with ID '{request.WorkerId}' was not found.");

        issue.AssignedWorkerId = worker.Id;
        issue.ChangeStatus(IssueStatus.WorkerAssigned, officerUserId, $"Assigned to field worker '{worker.FullName}'. Remarks: {request.Remarks}");

        if (worker.WorkerProfile != null)
        {
            worker.WorkerProfile.ActiveJobsCount += 1;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetIssueByIdAsync(issueId, cancellationToken);
    }

    public async Task<IssueResponseDto> AcceptJobAsync(Guid issueId, Guid workerUserId, CancellationToken cancellationToken = default)
    {
        var issue = await _context.Issues.Include(i => i.StatusHistory).FirstOrDefaultAsync(i => i.Id == issueId, cancellationToken);
        if (issue == null) throw new KeyNotFoundException($"Issue '{issueId}' not found.");

        if (issue.AssignedWorkerId != workerUserId)
        {
            throw new UnauthorizedAccessException("Only the assigned field worker can accept this job.");
        }

        issue.ChangeStatus(IssueStatus.Accepted, workerUserId, "Job accepted by field worker.");
        await _context.SaveChangesAsync(cancellationToken);

        return await GetIssueByIdAsync(issueId, cancellationToken);
    }

    public async Task<IssueResponseDto> StartWorkAsync(Guid issueId, Guid workerUserId, CancellationToken cancellationToken = default)
    {
        var issue = await _context.Issues.Include(i => i.StatusHistory).FirstOrDefaultAsync(i => i.Id == issueId, cancellationToken);
        if (issue == null) throw new KeyNotFoundException($"Issue '{issueId}' not found.");

        if (issue.AssignedWorkerId != workerUserId)
        {
            throw new UnauthorizedAccessException("Only the assigned field worker can start work on this job.");
        }

        issue.ChangeStatus(IssueStatus.InProgress, workerUserId, "Field worker arrived on site and started work.");
        await _context.SaveChangesAsync(cancellationToken);

        return await GetIssueByIdAsync(issueId, cancellationToken);
    }

    public async Task<IssueResponseDto> ResolveIssueAsync(Guid issueId, ResolveIssueRequestDto request, Guid workerUserId, CancellationToken cancellationToken = default)
    {
        var validationResult = await _resolveValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var issue = await _context.Issues.Include(i => i.Photos).Include(i => i.StatusHistory).FirstOrDefaultAsync(i => i.Id == issueId, cancellationToken);
        if (issue == null) throw new KeyNotFoundException($"Issue '{issueId}' not found.");

        if (issue.AssignedWorkerId != workerUserId)
        {
            throw new UnauthorizedAccessException("Only the assigned field worker can mark this issue as resolved.");
        }

        issue.ResolutionNotes = request.ResolutionNotes.Trim();

        // Attach completion photos
        if (request.AfterPhotoUrls != null && request.AfterPhotoUrls.Any())
        {
            foreach (var photoUrl in request.AfterPhotoUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                issue.Photos.Add(new IssuePhoto
                {
                    PhotoUrl = photoUrl,
                    PhotoType = PhotoType.WorkCompletedAfter,
                    UploadedByUserId = workerUserId,
                    UploadedAtUtc = DateTime.UtcNow,
                    Caption = "Completed work proof"
                });
            }
        }

        // Decrement worker active jobs count
        var worker = await _context.Users
            .Include(u => u.WorkerProfile)
            .FirstOrDefaultAsync(u => u.Id == workerUserId, cancellationToken);

        if (worker?.WorkerProfile != null)
        {
            if (worker.WorkerProfile.ActiveJobsCount > 0)
                worker.WorkerProfile.ActiveJobsCount -= 1;
            worker.WorkerProfile.TotalCompletedJobs += 1;
        }

        issue.ChangeStatus(IssueStatus.Resolved, workerUserId, $"Work completed. Notes: {request.ResolutionNotes}");

        await _context.SaveChangesAsync(cancellationToken);
        return await GetIssueByIdAsync(issueId, cancellationToken);
    }

    public async Task<IssueResponseDto> VerifyResolutionAsync(Guid issueId, VerifyIssueRequestDto request, Guid citizenUserId, CancellationToken cancellationToken = default)
    {
        var validationResult = await _verifyValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var issue = await _context.Issues.Include(i => i.StatusHistory).FirstOrDefaultAsync(i => i.Id == issueId, cancellationToken);
        if (issue == null) throw new KeyNotFoundException($"Issue '{issueId}' not found.");

        if (issue.ReportedByUserId != citizenUserId)
        {
            throw new UnauthorizedAccessException("Only the reporting citizen can verify the resolution.");
        }

        issue.CitizenFeedback = request.Feedback?.Trim();
        issue.CitizenRating = request.Rating;

        if (request.IsSatisfied)
        {
            issue.ChangeStatus(IssueStatus.Closed, citizenUserId, $"Citizen verified and approved resolution. Rating: {request.Rating ?? 5}/5. Feedback: {request.Feedback ?? "Satisfied"}");
        }
        else
        {
            issue.ChangeStatus(IssueStatus.Reopened, citizenUserId, $"Citizen rejected resolution and reopened issue. Feedback: {request.Feedback}");
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetIssueByIdAsync(issueId, cancellationToken);
    }

    public async Task<IssueResponseDto> ChangeStatusAsync(Guid issueId, ChangeStatusRequestDto request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var issue = await _context.Issues.Include(i => i.StatusHistory).FirstOrDefaultAsync(i => i.Id == issueId, cancellationToken);
        if (issue == null) throw new KeyNotFoundException($"Issue '{issueId}' not found.");

        issue.ChangeStatus(request.NewStatus, currentUserId, request.Remarks, request.MetadataJson);

        await _context.SaveChangesAsync(cancellationToken);
        return await GetIssueByIdAsync(issueId, cancellationToken);
    }

    public async Task<IssueResponseDto> MarkAsDuplicateAsync(Guid issueId, MarkDuplicateRequestDto request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var issue = await _context.Issues.Include(i => i.StatusHistory).FirstOrDefaultAsync(i => i.Id == issueId, cancellationToken);
        if (issue == null) throw new KeyNotFoundException($"Issue '{issueId}' not found.");

        var masterIssue = await _context.Issues.FindAsync(new object[] { request.MasterIssueId }, cancellationToken);
        if (masterIssue == null) throw new KeyNotFoundException($"Master Issue '{request.MasterIssueId}' not found.");

        issue.MasterIssueId = masterIssue.Id;
        issue.ChangeStatus(IssueStatus.Duplicate, currentUserId, $"Marked as duplicate of master issue {masterIssue.ReferenceNumber}. Remarks: {request.Remarks}");

        await _context.SaveChangesAsync(cancellationToken);
        return await GetIssueByIdAsync(issueId, cancellationToken);
    }

    private static IssueResponseDto MapToResponseDto(Issue issue)
    {
        return new IssueResponseDto(
            issue.Id,
            issue.ReferenceNumber,
            issue.Title,
            issue.Description,
            issue.AiSummary,
            issue.CategoryId,
            issue.Category?.Name ?? string.Empty,
            issue.Category?.PrimaryCategoryGroup ?? string.Empty,
            issue.DepartmentId,
            issue.Department?.Name,
            issue.ReportedByUserId,
            issue.ReportedByUser?.FullName ?? string.Empty,
            issue.AssignedWorkerId,
            issue.AssignedWorker?.FullName,
            issue.Status,
            issue.Status.ToString(),
            issue.Severity,
            issue.Severity.ToString(),
            issue.Priority,
            issue.Priority.ToString(),
            issue.Latitude,
            issue.Longitude,
            issue.Address,
            issue.WardSector,
            issue.ReportedAtUtc,
            issue.UpdatedAtUtc,
            issue.TargetSlaUtc,
            issue.ResolvedAtUtc,
            issue.ClosedAtUtc,
            issue.IsSensitive,
            issue.ResolutionNotes,
            issue.CitizenFeedback,
            issue.CitizenRating,
            issue.MasterIssueId,
            issue.Photos.OrderBy(p => p.UploadedAtUtc).Select(p => new IssuePhotoDto(
                p.Id,
                p.PhotoUrl,
                p.PhotoType,
                p.PhotoType.ToString(),
                p.Caption,
                p.UploadedByUserId,
                p.UploadedAtUtc
            )).ToList(),
            issue.StatusHistory.OrderBy(h => h.ChangedAtUtc).Select(h => new IssueStatusHistoryDto(
                h.Id,
                h.FromStatus,
                h.FromStatus.ToString(),
                h.ToStatus,
                h.ToStatus.ToString(),
                h.ChangedByUserId,
                h.ChangedByUser?.FullName ?? "System",
                h.ChangedAtUtc,
                h.Remarks
            )).ToList()
        );
    }

    private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371.0; // Earth radius in kilometers
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return r * c;
    }

    private static double ToRadians(double degrees) => degrees * (Math.PI / 180.0);
}
