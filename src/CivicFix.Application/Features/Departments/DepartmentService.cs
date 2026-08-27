using CivicFix.Application.Common.Interfaces;
using CivicFix.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CivicFix.Application.Features.Departments;

public class DepartmentService : IDepartmentService
{
    private readonly IApplicationDbContext _context;

    public DepartmentService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentDto>> GetAllDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _context.Departments
            .Include(d => d.Issues)
            .Include(d => d.StaffMembers)
                .ThenInclude(s => s.WorkerProfile)
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto(
                d.Id,
                d.Name,
                d.Code,
                d.Description,
                d.ContactEmail,
                d.ContactPhone,
                d.HeadOfficerName,
                d.Issues.Count(i => i.Status != IssueStatus.Closed && i.Status != IssueStatus.Rejected && i.Status != IssueStatus.Duplicate),
                d.StaffMembers.Count(s => s.Role == UserRole.FieldWorker && s.IsActive && s.WorkerProfile != null && s.WorkerProfile.IsAvailable)
            ))
            .ToListAsync(cancellationToken);

        return departments;
    }

    public async Task<DepartmentDto> GetDepartmentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var d = await _context.Departments
            .Include(d => d.Issues)
            .Include(d => d.StaffMembers)
                .ThenInclude(s => s.WorkerProfile)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (d == null)
        {
            throw new KeyNotFoundException($"Department with ID '{id}' was not found.");
        }

        return new DepartmentDto(
            d.Id,
            d.Name,
            d.Code,
            d.Description,
            d.ContactEmail,
            d.ContactPhone,
            d.HeadOfficerName,
            d.Issues.Count(i => i.Status != IssueStatus.Closed && i.Status != IssueStatus.Rejected && i.Status != IssueStatus.Duplicate),
            d.StaffMembers.Count(s => s.Role == UserRole.FieldWorker && s.IsActive && s.WorkerProfile != null && s.WorkerProfile.IsAvailable)
        );
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync(Guid? departmentId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories
            .Include(c => c.Department)
            .Where(c => c.IsActive)
            .AsQueryable();

        if (departmentId.HasValue)
        {
            query = query.Where(c => c.DepartmentId == departmentId.Value);
        }

        return await query
            .OrderBy(c => c.PrimaryCategoryGroup)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Code,
                c.PrimaryCategoryGroup,
                c.Description,
                c.DepartmentId,
                c.Department.Name,
                c.DefaultSlaHours
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<WorkerSummaryDto>> GetWorkersByDepartmentAsync(Guid departmentId, string? wardSector = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .Include(u => u.WorkerProfile)
            .Where(u => u.DepartmentId == departmentId && u.Role == UserRole.FieldWorker && u.IsActive)
            .AsQueryable();

        var workers = await query.ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(wardSector))
        {
            var sector = wardSector.Trim().ToLowerInvariant();
            // Prioritize workers matching the ward/sector, then sort by least active jobs
            return workers
                .OrderByDescending(w => w.WorkerProfile != null && w.WorkerProfile.AssignedWardOrZone.ToLowerInvariant().Contains(sector))
                .ThenBy(w => w.WorkerProfile?.ActiveJobsCount ?? 0)
                .Select(w => new WorkerSummaryDto(
                    w.Id,
                    w.FullName,
                    w.Email,
                    w.PhoneNumber,
                    w.WorkerProfile?.Specialization ?? "General",
                    w.WorkerProfile?.AssignedWardOrZone ?? "General",
                    w.WorkerProfile?.ActiveJobsCount ?? 0,
                    w.WorkerProfile?.MaxCapacity ?? 5,
                    w.WorkerProfile?.IsAvailable ?? true,
                    w.WorkerProfile?.Rating ?? 5.0,
                    w.WorkerProfile?.TotalCompletedJobs ?? 0
                ))
                .ToList();
        }

        return workers
            .OrderBy(w => w.WorkerProfile?.ActiveJobsCount ?? 0)
            .Select(w => new WorkerSummaryDto(
                w.Id,
                w.FullName,
                w.Email,
                w.PhoneNumber,
                w.WorkerProfile?.Specialization ?? "General",
                w.WorkerProfile?.AssignedWardOrZone ?? "General",
                w.WorkerProfile?.ActiveJobsCount ?? 0,
                w.WorkerProfile?.MaxCapacity ?? 5,
                w.WorkerProfile?.IsAvailable ?? true,
                w.WorkerProfile?.Rating ?? 5.0,
                w.WorkerProfile?.TotalCompletedJobs ?? 0
            ))
            .ToList();
    }
}
