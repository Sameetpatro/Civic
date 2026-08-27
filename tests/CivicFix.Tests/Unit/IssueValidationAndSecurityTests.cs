using CivicFix.Application.Features.Issues;
using CivicFix.Domain.Entities;
using CivicFix.Domain.Enums;
using CivicFix.Infrastructure.Data;
using FluentAssertions;
using FluentValidation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CivicFix.Tests.Unit;

public class IssueValidationAndSecurityTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly CivicFixDbContext _dbContext;
    private readonly IssueService _issueService;
    private readonly Department _waterDepartment;
    private readonly Category _leakCategory;
    private readonly User _citizenUser;
    private readonly User _otherCitizen;

    public IssueValidationAndSecurityTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<CivicFixDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new CivicFixDbContext(options);
        _dbContext.Database.EnsureCreated();

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
            FullName = "Citizen One",
            Email = "citizen1@example.com",
            PhoneNumber = "+91-9876540001",
            PasswordHash = "hash",
            Role = UserRole.Citizen
        };

        _otherCitizen = new User
        {
            FullName = "Citizen Two",
            Email = "citizen2@example.com",
            PhoneNumber = "+91-9876540002",
            PasswordHash = "hash",
            Role = UserRole.Citizen
        };

        _dbContext.Users.AddRange(_citizenUser, _otherCitizen);
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
    public async Task CreateIssueAsync_ShouldThrowValidationException_WhenDescriptionIsTooShort()
    {
        // Arrange
        var request = new CreateIssueRequestDto(
            Title: "Broken Pipe",
            Description: "Short", // Less than 10 characters
            CategoryId: _leakCategory.Id,
            Latitude: 28.9931,
            Longitude: 77.0151,
            Address: "Main Road",
            WardSector: "Sector 14"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _issueService.CreateIssueAsync(request, _citizenUser.Id));
    }

    [Fact]
    public async Task VerifyResolution_ShouldThrowUnauthorized_WhenNonReportingCitizenAttemptsVerification()
    {
        // Arrange
        var createRequest = new CreateIssueRequestDto(
            Title: "Main Water Line Burst in Market",
            Description: "Water line broken causing massive water logging.",
            CategoryId: _leakCategory.Id,
            Latitude: 28.9931,
            Longitude: 77.0151,
            Address: "Sector 14 Market",
            WardSector: "Sector 14"
        );
        var created = await _issueService.CreateIssueAsync(createRequest, _citizenUser.Id);

        var verifyRequest = new VerifyIssueRequestDto(IsSatisfied: true, Feedback: "Good", Rating: 5);

        // Act & Assert: Citizen Two attempts to verify Citizen One's report
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _issueService.VerifyResolutionAsync(created.Id, verifyRequest, _otherCitizen.Id));
    }

    [Fact]
    public async Task GetNearbyIssuesAsync_ShouldFilterByHaversineRadius()
    {
        // Arrange: Create an issue in Sector 14 Sonipat (approx 28.9931, 77.0151)
        var closeRequest = new CreateIssueRequestDto(
            Title: "Close Issue Sector 14",
            Description: "Issue 200 meters away from citizen location.",
            CategoryId: _leakCategory.Id,
            Latitude: 28.9935,
            Longitude: 77.0155,
            Address: "Near Community Center, Sector 14",
            WardSector: "Sector 14"
        );
        await _issueService.CreateIssueAsync(closeRequest, _citizenUser.Id);

        // Create another issue far away (e.g. Delhi border, approx 35km away: 28.7041, 77.1025)
        var farRequest = new CreateIssueRequestDto(
            Title: "Far Away Issue",
            Description: "Issue situated 35km away in outer region.",
            CategoryId: _leakCategory.Id,
            Latitude: 28.7041,
            Longitude: 77.1025,
            Address: "Outer Highway",
            WardSector: "Outer Ward"
        );
        await _issueService.CreateIssueAsync(farRequest, _citizenUser.Id);

        // Act: Search within 2km radius of Sector 14
        var search = new NearbyIssuesRequestDto(28.9931, 77.0151, RadiusKm: 2.0);
        var nearby = await _issueService.GetNearbyIssuesAsync(search);

        // Assert: Only the close issue is returned
        nearby.Should().HaveCount(1);
        nearby.First().Title.Should().Be("Close Issue Sector 14");
    }

    public void Dispose()
    {
        _connection.Dispose();
        _dbContext.Dispose();
    }
}
