using CivicFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicFix.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Department> Departments { get; }
    DbSet<Category> Categories { get; }
    DbSet<WorkerProfile> WorkerProfiles { get; }
    DbSet<Issue> Issues { get; }
    DbSet<IssuePhoto> IssuePhotos { get; }
    DbSet<IssueStatusHistory> IssueStatusHistories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
