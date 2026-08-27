using CivicFix.Application.Common.Interfaces;
using CivicFix.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CivicFix.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var idClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(idClaim, out var id) ? id : null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public UserRole? Role
    {
        get
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : null;
        }
    }

    public Guid? DepartmentId
    {
        get
        {
            var deptClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("DepartmentId");
            return Guid.TryParse(deptClaim, out var deptId) ? deptId : null;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
