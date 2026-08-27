namespace CivicFix.Application.Features.Departments;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<DepartmentDto> GetDepartmentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<CategoryDto>> GetAllCategoriesAsync(Guid? departmentId = null, CancellationToken cancellationToken = default);
    Task<List<WorkerSummaryDto>> GetWorkersByDepartmentAsync(Guid departmentId, string? wardSector = null, CancellationToken cancellationToken = default);
}
