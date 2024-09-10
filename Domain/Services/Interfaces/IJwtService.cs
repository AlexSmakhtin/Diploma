using System.Security.Claims;
using Domain.Entities.Implementations;

namespace Domain.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateEmailToken(User user);
    ClaimsPrincipal ValidateEmailToken(string token);
}