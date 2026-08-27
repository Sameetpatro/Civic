using CivicFix.Domain.Entities;

namespace CivicFix.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
