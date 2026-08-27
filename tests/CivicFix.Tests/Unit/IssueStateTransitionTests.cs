using CivicFix.Domain.Entities;
using CivicFix.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CivicFix.Tests.Unit;

public class IssueStateTransitionTests
{
    [Fact]
    public void ChangeStatus_ShouldRecordAuditTrailAndTimestamps_WhenStatusChanges()
    {
        // Arrange
        var citizenId = Guid.NewGuid();
        var officerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();

        var issue = new Issue
        {
            ReferenceNumber = "CVX-20260827-1001",
            Title = "Severe Main Pipeline Leak",
            Description = "Water gushing near Gate 2",
            ReportedByUserId = citizenId,
            Status = IssueStatus.Reported,
            WardSector = "Sector 14"
        };

        // Act 1: Officer assigns department & worker
        var history1 = issue.ChangeStatus(IssueStatus.DepartmentAssigned, officerId, "Assigned to Water Dept");
        var history2 = issue.ChangeStatus(IssueStatus.WorkerAssigned, officerId, "Assigned to Worker Ramesh");

        // Act 2: Worker accepts and starts
        var history3 = issue.ChangeStatus(IssueStatus.Accepted, workerId, "Worker accepted");
        var history4 = issue.ChangeStatus(IssueStatus.InProgress, workerId, "Worker on site");

        // Act 3: Worker resolves
        var history5 = issue.ChangeStatus(IssueStatus.Resolved, workerId, "Replaced broken valve collar");

        // Act 4: Citizen verifies and closes
        var history6 = issue.ChangeStatus(IssueStatus.Closed, citizenId, "Citizen confirmed water supply restored");

        // Assert
        issue.Status.Should().Be(IssueStatus.Closed);
        issue.ResolvedAtUtc.Should().NotBeNull();
        issue.ClosedAtUtc.Should().NotBeNull();
        issue.StatusHistory.Should().HaveCount(6);

        history1.FromStatus.Should().Be(IssueStatus.Reported);
        history1.ToStatus.Should().Be(IssueStatus.DepartmentAssigned);

        history5.ToStatus.Should().Be(IssueStatus.Resolved);
        history6.ToStatus.Should().Be(IssueStatus.Closed);
    }
}
