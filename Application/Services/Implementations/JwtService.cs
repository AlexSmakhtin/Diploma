using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Configurations;
using Domain.Entities.Implementations;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services.Implementations;

public class JwtService: IJwtService
{
    private readonly JwtConfig _jwtConfig;

    public JwtService(IOptions<JwtConfig> jwtConfig)
    {
        ArgumentNullException.ThrowIfNull(jwtConfig);
        _jwtConfig = jwtConfig.Value;
    }

    public string GenerateToken(User user)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = CreateClaimsIdentity(user),
            Expires = DateTime.UtcNow.Add(_jwtConfig.LifeTime),
            Audience = _jwtConfig.Audience,
            Issuer = _jwtConfig.Issuer,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_jwtConfig.SigningKeyBytes),
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }

    public string GenerateEmailToken(User user)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = CreateEmailClaimsIdentity(user),
            Expires = DateTime.UtcNow.Add(_jwtConfig.LifeTime),
            Audience = _jwtConfig.Audience,
            Issuer = _jwtConfig.Issuer,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_jwtConfig.SigningKeyBytes),
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }
    
    private ClaimsIdentity CreateClaimsIdentity(User user)
    {
        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        });
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, user.Status.ToString()));

        return claimsIdentity;
    }

    private ClaimsIdentity CreateEmailClaimsIdentity(User user)
    {
        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        });

        return claimsIdentity;
    }

    public ClaimsPrincipal ValidateEmailToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(_jwtConfig.SigningKeyBytes),
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = _jwtConfig.Issuer,
            ValidAudiences =  new[] { _jwtConfig.Audience},
            ValidateLifetime = true
        }, out var validatedToken);
        return principal;
    }
}