using CivicFix.Domain.Enums;

namespace CivicFix.Application.Features.Auth;

public record RegisterRequestDto(
    string FullName,
    string Email,
    string Password,
    string PhoneNumber,
    UserRole Role = UserRole.Citizen,
    Guid? DepartmentId = null
);

public record LoginRequestDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    string Token,
    UserDto User,
    DateTime ExpiresAtUtc
);

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    UserRole Role,
    string RoleName,
    Guid? DepartmentId,
    string? DepartmentName,
    DateTime CreatedAtUtc
);
