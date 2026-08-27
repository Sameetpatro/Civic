using CivicFix.Application.Common.Interfaces;
using CivicFix.Domain.Entities;
using CivicFix.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CivicFix.Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;

    public AuthService(
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException($"A user with email '{request.Email}' already exists.");
        }

        // Validate Department if specified
        Department? department = null;
        if (request.DepartmentId.HasValue)
        {
            department = await _context.Departments.FindAsync(new object[] { request.DepartmentId.Value }, cancellationToken);
            if (department == null)
            {
                throw new KeyNotFoundException($"Department with ID '{request.DepartmentId.Value}' was not found.");
            }
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PhoneNumber = request.PhoneNumber.Trim(),
            PasswordHash = passwordHash,
            Role = request.Role,
            DepartmentId = request.DepartmentId,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        // If worker, create initial worker profile
        if (request.Role == UserRole.FieldWorker)
        {
            newUser.WorkerProfile = new WorkerProfile
            {
                Specialization = "General Maintenance",
                AssignedWardOrZone = "Sonipat Central",
                ActiveJobsCount = 0,
                MaxCapacity = 5,
                IsAvailable = true
            };
        }

        await _context.Users.AddAsync(newUser, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(newUser);
        var expiresAtUtc = DateTime.UtcNow.AddDays(7);

        var userDto = new UserDto(
            newUser.Id,
            newUser.FullName,
            newUser.Email,
            newUser.PhoneNumber,
            newUser.Role,
            newUser.Role.ToString(),
            newUser.DepartmentId,
            department?.Name,
            newUser.CreatedAtUtc
        );

        return new AuthResponseDto(token, userDto, expiresAtUtc);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (user == null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);
        var expiresAtUtc = DateTime.UtcNow.AddDays(7);

        var userDto = new UserDto(
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.Role,
            user.Role.ToString(),
            user.DepartmentId,
            user.Department?.Name,
            user.CreatedAtUtc
        );

        return new AuthResponseDto(token, userDto, expiresAtUtc);
    }

    public async Task<UserDto> GetCurrentUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID '{userId}' not found.");
        }

        return new UserDto(
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.Role,
            user.Role.ToString(),
            user.DepartmentId,
            user.Department?.Name,
            user.CreatedAtUtc
        );
    }
}
