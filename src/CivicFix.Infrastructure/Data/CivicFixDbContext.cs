using CivicFix.Application.Common.Interfaces;
using CivicFix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CivicFix.Infrastructure.Data;

public class CivicFixDbContext : DbContext, IApplicationDbContext
{
    public CivicFixDbContext(DbContextOptions<CivicFixDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<WorkerProfile> WorkerProfiles => Set<WorkerProfile>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<IssuePhoto> IssuePhotos => Set<IssuePhoto>();
    public DbSet<IssueStatusHistory> IssueStatusHistories => Set<IssueStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
