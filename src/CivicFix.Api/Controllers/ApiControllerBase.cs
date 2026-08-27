using CivicFix.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CivicFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(idClaim, out var id))
            {
                return id;
            }
            throw new UnauthorizedAccessException("User is not authenticated or has an invalid token.");
        }
    }

    protected UserRole? CurrentUserRole
    {
        get
        {
            var roleClaim = User.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : null;
        }
    }

    protected Guid? CurrentUserDepartmentId
    {
        get
        {
            var deptClaim = User.FindFirstValue("DepartmentId");
            return Guid.TryParse(deptClaim, out var deptId) ? deptId : null;
        }
    }
}
