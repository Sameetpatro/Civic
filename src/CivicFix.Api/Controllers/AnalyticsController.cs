using System.Text;
using CivicFix.Application.Common.Interfaces;
using CivicFix.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CivicFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;

    public AnalyticsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("kpis")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKpis(CancellationToken cancellationToken)
    {
        var totalIssues = await _context.Issues.CountAsync(cancellationToken);
        var inProgress = await _context.Issues.CountAsync(i => 
            i.Status == IssueStatus.InProgress || 
            i.Status == IssueStatus.Accepted || 
            i.Status == IssueStatus.WorkerAssigned, 
            cancellationToken);

        var resolved = await _context.Issues.CountAsync(i => 
            i.Status == IssueStatus.Resolved || 
            i.Status == IssueStatus.Closed, 
            cancellationToken);

        var critical = await _context.Issues.CountAsync(i => 
            i.Severity == IssueSeverity.Critical || 
            i.Severity == IssueSeverity.High, 
            cancellationToken);

        var now = DateTime.UtcNow;
        var slaBreaches = await _context.Issues.CountAsync(i => 
            i.Status != IssueStatus.Closed && 
            i.Status != IssueStatus.Resolved && 
            i.TargetSlaUtc < now, 
            cancellationToken);

        var avgRating = await _context.Issues
            .Where(i => i.CitizenRating.HasValue)
            .AverageAsync(i => (double?)i.CitizenRating, cancellationToken) ?? 4.5;

        return Ok(new
        {
            TotalReported = totalIssues,
            ActiveInProgress = inProgress,
            ResolvedAndClosed = resolved,
            CriticalHazards = critical,
            SlaOverdueBreaches = slaBreaches,
            AverageCitizenRating = Math.Round(avgRating, 2),
            SlaCompliancePercentage = totalIssues > 0 ? Math.Round((1.0 - ((double)slaBreaches / totalIssues)) * 100, 1) : 100.0,
            Region = "Sonipat, Haryana, India"
        });
    }

    [HttpGet("export/csv")]
    [Produces("text/csv")]
    public async Task<IActionResult> ExportIncidentsCsv(CancellationToken cancellationToken)
    {
        var issues = await _context.Issues
            .Include(i => i.Category)
            .Include(i => i.Department)
            .Include(i => i.ReportedByUser)
            .Include(i => i.AssignedWorker)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine("ReferenceNumber,Title,Department,Category,Status,Severity,Priority,WardSector,Latitude,Longitude,ReportedAtUtc,TargetSlaUtc,ResolvedAtUtc,CitizenRating,IsSensitive");

        foreach (var i in issues)
        {
            var cleanTitle = i.Title.Replace(",", " ").Replace("\"", "");
            var dept = i.Department?.Name ?? "Unassigned";
            var cat = i.Category?.Name ?? "General";
            var worker = i.AssignedWorker?.FullName ?? "Unassigned";

            builder.AppendLine($"{i.ReferenceNumber},\"{cleanTitle}\",\"{dept}\",\"{cat}\",{i.Status},{i.Severity},{i.Priority},\"{i.WardSector}\",{i.Latitude},{i.Longitude},{i.ReportedAtUtc:yyyy-MM-dd HH:mm:ss},{i.TargetSlaUtc:yyyy-MM-dd HH:mm:ss},{i.ResolvedAtUtc:yyyy-MM-dd HH:mm:ss},{i.CitizenRating?.ToString() ?? ""},{i.IsSensitive}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(builder.ToString());
        return File(csvBytes, "text/csv", $"civicfix_sonipat_incidents_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpGet("ward-performance")]
    public async Task<IActionResult> GetWardPerformance(CancellationToken cancellationToken)
    {
        var performance = await _context.Issues
            .GroupBy(i => i.WardSector)
            .Select(g => new
            {
                Ward = g.Key,
                TotalIncidents = g.Count(),
                ActiveIncidents = g.Count(i => i.Status != IssueStatus.Closed && i.Status != IssueStatus.Resolved),
                ResolvedIncidents = g.Count(i => i.Status == IssueStatus.Closed || i.Status == IssueStatus.Resolved),
                AverageRating = g.Where(i => i.CitizenRating.HasValue).Average(i => (double?)i.CitizenRating) ?? 4.2
            })
            .ToListAsync(cancellationToken);

        return Ok(performance);
    }
}
