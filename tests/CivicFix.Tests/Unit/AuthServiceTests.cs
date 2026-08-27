using CivicFix.Application.Common.Interfaces;
using CivicFix.Application.Features.Auth;
using CivicFix.Domain.Entities;
using CivicFix.Domain.Enums;
using CivicFix.Infrastructure.Data;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CivicFix.Tests.Unit;

public class AuthServiceTests
{
    private readonly CivicFixDbContext _dbContext;
    private readonly Mock<IJwtTokenGenerator> _mockJwtGenerator;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<CivicFixDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CivicFixDbContext(options);
        _mockJwtGenerator = new Mock<IJwtTokenGenerator>();
        _mockJwtGenerator.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("fake_jwt_token_12345");

        _registerValidator = new RegisterRequestValidator();
        _loginValidator = new LoginRequestValidator();

        _authService = new AuthService(_dbContext, _mockJwtGenerator.Object, _registerValidator, _loginValidator);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndReturnToken_WhenRequestIsValid()
    {
        // Arrange
        var request = new RegisterRequestDto(
            FullName: "Aakash Gupta",
            Email: "aakash.gupta@example.com",
            Password: "Password123!",
            PhoneNumber: "+91-9876543210",
            Role: UserRole.Citizen
        );

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("fake_jwt_token_12345");
        result.User.Email.Should().Be("aakash.gupta@example.com");
        result.User.Role.Should().Be(UserRole.Citizen);

        var savedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "aakash.gupta@example.com");
        savedUser.Should().NotBeNull();
        BCrypt.Net.BCrypt.Verify("Password123!", savedUser!.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("SecurePass123!");
        var user = new User
        {
            FullName = "Rohan Verma",
            Email = "rohan.verma@example.com",
            PhoneNumber = "+91-9876543211",
            PasswordHash = passwordHash,
            Role = UserRole.Citizen,
            IsActive = true
        };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequestDto("rohan.verma@example.com", "SecurePass123!");

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("fake_jwt_token_12345");
        result.User.Email.Should().Be("rohan.verma@example.com");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenPasswordIsIncorrect()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!");
        var user = new User
        {
            FullName = "Kavita Rao",
            Email = "kavita.rao@example.com",
            PhoneNumber = "+91-9876543212",
            PasswordHash = passwordHash,
            Role = UserRole.Citizen,
            IsActive = true
        };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequestDto("kavita.rao@example.com", "WrongPassword999!");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginRequest));
    }
}
