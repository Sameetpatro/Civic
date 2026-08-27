using CivicFix.Application.Features.Auth;
using CivicFix.Application.Features.Departments;
using CivicFix.Application.Features.Issues;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace CivicFix.Tests.Integration;

public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDepartments_ShouldReturnSeededDepartments()
    {
        // Act
        var response = await _client.GetAsync("/api/departments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var departments = await response.Content.ReadFromJsonAsync<List<DepartmentDto>>();
        departments.Should().NotBeNull();
        departments.Should().Contain(d => d.Code == "WATER");
        departments.Should().Contain(d => d.Code == "ROADS");
        departments.Should().Contain(d => d.Code == "GARBAGE");
    }

    [Fact]
    public async Task FullCitizenReportingFlow_ThroughApiEndpoints()
    {
        // 1. Login as citizen
        var loginPayload = new LoginRequestDto("vikram.singh@gmail.com", "Password123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        authResult.Should().NotBeNull();
        authResult!.Token.Should().NotBeNullOrEmpty();

        // 2. Fetch categories to get a valid CategoryId
        var categoriesResponse = await _client.GetAsync("/api/departments/categories");
        categoriesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<CategoryDto>>(_jsonOptions);
        var potholeCategory = categories!.First(c => c.Code == "ROAD_POTHOLE");

        // 3. Create Issue with Citizen Bearer Token
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Token);

        var createIssuePayload = new CreateIssueRequestDto(
            Title: "Dangerous Deep Pothole on Main Road",
            Description: "Large pothole near Sector 14 market roundabout causing traffic congestion.",
            CategoryId: potholeCategory.Id,
            Latitude: 28.9950,
            Longitude: 77.0160,
            Address: "Sector 14 Roundabout, Sonipat",
            WardSector: "Sector 14",
            PhotoUrls: new List<string> { "https://example.com/pothole.jpg" }
        );

        var createResponse = await _client.PostAsJsonAsync("/api/issues", createIssuePayload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdIssue = await createResponse.Content.ReadFromJsonAsync<IssueResponseDto>(_jsonOptions);
        createdIssue.Should().NotBeNull();
        createdIssue!.ReferenceNumber.Should().StartWith("CVX-");
        createdIssue.DepartmentName.Should().Contain("Road");
        createdIssue.ReportedByUserName.Should().Be("Vikram Singh");

        // 4. Fetch the created issue by ID
        var getResponse = await _client.GetAsync($"/api/issues/{createdIssue.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetchedIssue = await getResponse.Content.ReadFromJsonAsync<IssueResponseDto>(_jsonOptions);
        fetchedIssue.Should().NotBeNull();
        fetchedIssue!.Id.Should().Be(createdIssue.Id);
        fetchedIssue.StatusHistory.Should().NotBeEmpty();
    }
}
