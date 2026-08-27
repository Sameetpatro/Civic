using CivicFix.Domain.Enums;

namespace CivicFix.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    UserRole? Role { get; }
    Guid? DepartmentId { get; }
    bool IsAuthenticated { get; }
}
