namespace CivicFix.Application.Features.Issues;

public interface IIssueService
{
    Task<IssueResponseDto> CreateIssueAsync(CreateIssueRequestDto request, Guid reportingUserId, CancellationToken cancellationToken = default);
    Task<IssueResponseDto> GetIssueByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResultDto<IssueSummaryDto>> GetIssuesAsync(IssueFilterDto filter, CancellationToken cancellationToken = default);
    Task<List<IssueSummaryDto>> GetNearbyIssuesAsync(NearbyIssuesRequestDto request, CancellationToken cancellationToken = default);

    // Lifecycle State Transitions
    Task<IssueResponseDto> AssignDepartmentAsync(Guid issueId, AssignDepartmentRequestDto request, Guid officerUserId, CancellationToken cancellationToken = default);
    Task<IssueResponseDto> AssignWorkerAsync(Guid issueId, AssignWorkerRequestDto request, Guid officerUserId, CancellationToken cancellationToken = default);
    Task<IssueResponseDto> AcceptJobAsync(Guid issueId, Guid workerUserId, CancellationToken cancellationToken = default);
    Task<IssueResponseDto> StartWorkAsync(Guid issueId, Guid workerUserId, CancellationToken cancellationToken = default);
    Task<IssueResponseDto> ResolveIssueAsync(Guid issueId, ResolveIssueRequestDto request, Guid workerUserId, CancellationToken cancellationToken = default);
    Task<IssueResponseDto> VerifyResolutionAsync(Guid issueId, VerifyIssueRequestDto request, Guid citizenUserId, CancellationToken cancellationToken = default);
    Task<IssueResponseDto> ChangeStatusAsync(Guid issueId, ChangeStatusRequestDto request, Guid currentUserId, CancellationToken cancellationToken = default);
    Task<IssueResponseDto> MarkAsDuplicateAsync(Guid issueId, MarkDuplicateRequestDto request, Guid currentUserId, CancellationToken cancellationToken = default);
}
