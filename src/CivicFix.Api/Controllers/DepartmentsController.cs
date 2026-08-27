using CivicFix.Application.Features.Departments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicFix.Api.Controllers;

public class DepartmentsController : ApiControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<DepartmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments(CancellationToken cancellationToken)
    {
        var result = await _departmentService.GetAllDepartmentsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartmentById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _departmentService.GetDepartmentByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories([FromQuery] Guid? departmentId, CancellationToken cancellationToken)
    {
        var result = await _departmentService.GetAllCategoriesAsync(departmentId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/workers")]
    [Authorize(Roles = "DepartmentOfficer,Admin")]
    [ProducesResponseType(typeof(List<WorkerSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkers(Guid id, [FromQuery] string? wardSector, CancellationToken cancellationToken)
    {
        var result = await _departmentService.GetWorkersByDepartmentAsync(id, wardSector, cancellationToken);
        return Ok(result);
    }
}
