using CivicFix.Application.Features.Issues;
using CivicFix.Domain.Entities;
using CivicFix.Domain.Enums;
using CivicFix.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CivicFix.Tests.Unit;

public class IssueServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly CivicFixDbContext _dbContext;
    private readonly IssueService _issueService;
    private readonly Department _waterDepartment;
    private readonly Category _leakCategory;
    private readonly User _citizenUser;
    private readonly User _officerUser;
    private readonly User _workerUser;

    public IssueServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<CivicFixDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new CivicFixDbContext(options);
        _dbContext.Database.EnsureCreated();

        // Seed base setup
        _waterDepartment = new Department
        {
            Name = "Water Department",
            Code = "WATER",
            Description = "Water issues"
        };
        _dbContext.Departments.Add(_waterDepartment);

        _leakCategory = new Category
        {
            Name = "Pipe Leakage",
            Code = "WATER_PIPE_LEAK",
            PrimaryCategoryGroup = "WATER",
            Department = _waterDepartment,
            DefaultSlaHours = 24
        };
        _dbContext.Categories.Add(_leakCategory);

        _citizenUser = new User
        {
            FullName = "Aman Verma",
            Email = "aman@example.com",
            PhoneNumber = "+91-9876540001",
            PasswordHash = "hash",
            Role = UserRole.Citizen
        };

        _officerUser = new User
        {
            FullName = "Officer Malik",
            Email = "malik@example.com",
            PhoneNumber = "+91-9876540002",
            PasswordHash = "hash",
            Role = UserRole.DepartmentOfficer,
            Department = _waterDepartment
        };

        var workerUserId = Guid.NewGuid();
        _workerUser = new User
        {
            Id = workerUserId,
            FullName = "Worker Ramesh",
            Email = "ramesh@example.com",
            PhoneNumber = "+91-9876540003",
            PasswordHash = "hash",
            Role = UserRole.FieldWorker,
            Department = _waterDepartment,
            WorkerProfile = new WorkerProfile
            {
                UserId = workerUserId,
                Specialization = "Plumbing",
                AssignedWardOrZone = "Sector 14",
                ActiveJobsCount = 0,
                MaxCapacity = 5,
                IsAvailable = true
            }
        };

        _dbContext.Users.AddRange(_citizenUser, _officerUser, _workerUser);
        _dbContext.SaveChanges();
        _dbContext.ChangeTracker.Clear();

        _issueService = new IssueService(
            _dbContext,
            new CreateIssueRequestValidator(),
            new ResolveIssueRequestValidator(),
            new VerifyIssueRequestValidator()
        );
    }

    [Fact]
    public async Task CreateIssueAsync_ShouldAutoRouteToDepartmentAndCalculateSla()
    {
        // Arrange
        var request = new CreateIssueRequestDto(
            Title: "Broken Water Pipe near Community Center",
            Description: "Clean water overflowing onto the main road for 3 hours.",
            CategoryId: _leakCategory.Id,
            Latitude: 28.9931,
            Longitude: 77.0151,
            Address: "Community Center, Sector 14",
            WardSector: "Sector 14",
            PhotoUrls: new List<string> { "https://civicfix.blob.core.windows.net/photos/leak1.jpg" }
        );

        // Act
        var result = await _issueService.CreateIssueAsync(request, _citizenUser.Id);

        // Assert
        result.Should().NotBeNull();
        result.ReferenceNumber.Should().StartWith("CVX-");
        result.Status.Should().Be(IssueStatus.DepartmentAssigned);
        result.DepartmentId.Should().Be(_waterDepartment.Id);
        result.DepartmentName.Should().Be("Water Department");
        result.Photos.Should().HaveCount(1);
        result.TargetSlaUtc.Should().BeAfter(result.ReportedAtUtc);
        result.StatusHistory.Should().HaveCount(1);
    }

    [Fact]
    public async Task CompleteIssueLifecycle_ShouldTrackWorkerWorkloadAndCitizenApproval()
    {
        var profile = await _dbContext.WorkerProfiles.FirstAsync();
        profile.UserId.Should().Be(_workerUser.Id);

        // 1. Citizen creates issue
        var createRequest = new CreateIssueRequestDto(
            Title: "Main Water Line Burst",
            Description: "Heavy water pressure causing road sinkhole.",
            CategoryId: _leakCategory.Id,
            Latitude: 28.9931,
            Longitude: 77.0151,
            Address: "Main Road, Sector 14",
            WardSector: "Sector 14"
        );
        var created = await _issueService.CreateIssueAsync(createRequest, _citizenUser.Id);

        // 2. Officer assigns worker
        var assignWorkerRequest = new AssignWorkerRequestDto(_workerUser.Id, "Assigned to Ramesh for urgent repair");
        var afterAssign = await _issueService.AssignWorkerAsync(created.Id, assignWorkerRequest, _officerUser.Id);
        afterAssign.Status.Should().Be(IssueStatus.WorkerAssigned);

        var workerProfile = await _dbContext.WorkerProfiles.FirstAsync(w => w.UserId == _workerUser.Id);
        workerProfile.ActiveJobsCount.Should().Be(1);

        // 3. Worker accepts job
        var accepted = await _issueService.AcceptJobAsync(created.Id, _workerUser.Id);
        accepted.Status.Should().Be(IssueStatus.Accepted);

        // 4. Worker starts work
        var inProgress = await _issueService.StartWorkAsync(created.Id, _workerUser.Id);
        inProgress.Status.Should().Be(IssueStatus.InProgress);

        // 5. Worker resolves issue with evidence
        var resolveRequest = new ResolveIssueRequestDto(
            ResolutionNotes: "Welded new pipeline segment and sealed the joint.",
            AfterPhotoUrls: new List<string> { "https://civicfix.blob.core.windows.net/photos/fixed.jpg" }
        );
        var resolved = await _issueService.ResolveIssueAsync(created.Id, resolveRequest, _workerUser.Id);
        resolved.Status.Should().Be(IssueStatus.Resolved);
        resolved.ResolutionNotes.Should().Be("Welded new pipeline segment and sealed the joint.");
        workerProfile.ActiveJobsCount.Should().Be(0);
        workerProfile.TotalCompletedJobs.Should().Be(1);

        // 6. Citizen verifies and approves
        var verifyRequest = new VerifyIssueRequestDto(
            IsSatisfied: true,
            Feedback: "Excellent fast response, water pressure is normal now!",
            Rating: 5
        );
        var closed = await _issueService.VerifyResolutionAsync(created.Id, verifyRequest, _citizenUser.Id);
        closed.Status.Should().Be(IssueStatus.Closed);
        closed.CitizenRating.Should().Be(5);
        closed.CitizenFeedback.Should().Be("Excellent fast response, water pressure is normal now!");
        closed.StatusHistory.Should().HaveCount(6);
    }

    public void Dispose()
    {
        _connection.Dispose();
        _dbContext.Dispose();
    }
}
