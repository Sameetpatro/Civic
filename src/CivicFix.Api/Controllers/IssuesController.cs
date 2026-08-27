using CivicFix.Application.Features.Issues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicFix.Api.Controllers;

public class IssuesController : ApiControllerBase
{
    private readonly IIssueService _issueService;

    public IssuesController(IIssueService issueService)
    {
        _issueService = issueService;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(IssueResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateIssue([FromBody] CreateIssueRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _issueService.CreateIssueAsync(request, CurrentUserId, cancellationToken);
        return CreatedAtAction(nameof(GetIssueById), new { id = result.Id }, result);
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResultDto<IssueSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIssues([FromQuery] IssueFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _issueService.GetIssuesAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(IssueResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIssueById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _issueService.GetIssueByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("nearby")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<IssueSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNearbyIssues([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] double radiusKm = 2.0, CancellationToken cancellationToken = default)
    {
        var request = new NearbyIssuesRequestDto(latitude, longitude, radiusKm);
        var result = await _issueService.GetNearbyIssuesAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/assign-department")]
    [Authorize(Roles = "DepartmentOfficer,Admin")]
    [ProducesResponseType(typeof(IssueResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignDepartment(Guid id, [FromBody] AssignDepartmentRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _issueService.AssignDepartmentAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/assign-worker")]
    [Authorize(Roles = "DepartmentOfficer,Admin")]
    [ProducesResponseType(typeof(IssueResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignWorker(Guid id, [FromBody] AssignWorkerRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _issueService.AssignWorkerAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "FieldWorker")]
    [ProducesResponseType(typeof(IssueResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AcceptJob(Guid id, CancellationToken cancellationToken)
    {
        var result = await _issueService.AcceptJobAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/start")]
    [Authorize(Roles = "FieldWorker")]
    [ProducesResponseType(typeof(IssueResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> StartWork(Guid id, CancellationToken cancellationToken)
    {
        var result = await _issueService.StartWorkAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/resolve")]
    [Authorize(Roles = "FieldWorker")]
    [ProducesResponseType(typeof(IssueResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResolveIssue(Guid id, [FromBody] ResolveIssueRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _issueService.ResolveIssueAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/verify")]
    [Authorize(Roles = "Citizen")]
    [ProducesResponseType(typeof(IssueResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyResolution(Guid id, [FromBody] VerifyIssueRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _issueService.VerifyResolutionAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/status")]
    [Authorize(Roles = "DepartmentOfficer,Admin")]
    [ProducesResponseType(typeof(IssueResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _issueService.ChangeStatusAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/duplicate")]
    [Authorize(Roles = "DepartmentOfficer,Admin")]
    [ProducesResponseType(typeof(IssueResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkDuplicate(Guid id, [FromBody] MarkDuplicateRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _issueService.MarkAsDuplicateAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
