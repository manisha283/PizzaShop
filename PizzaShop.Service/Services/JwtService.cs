using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Service.Services;

public class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _tokenDuration;

    public JwtService(IConfiguration configuration)
    {
        _secretKey = configuration["JwtConfig:Key"] ?? throw new ArgumentNullException("JwtConfig:Key is missing.");
        _issuer = configuration["JwtConfig:Issuer"] ?? "localhost";
        _audience = configuration["JwtConfig:Audience"] ?? "localhost";
        _tokenDuration = int.Parse(configuration["JwtConfig:Duration"] ?? "24"); // Default: 24 hours
    }

    public string GenerateToken(string email, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("email", email),
            new Claim("role", role)   
        };

        var token = new JwtSecurityToken(
            issuer: "localhost",
            audience: "localhost",
            claims: claims,
            expires: DateTime.Now.AddHours(_tokenDuration),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Extracts claims from a JWT token.
    public ClaimsPrincipal? GetClaimsFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true
            };

            var principal = tokenHandler.ValidateToken(token, parameters, out _);
            return principal;
        }
        catch (Exception)
        {
            return null; // Token is invalid
        }
    }

    // Retrieves a specific claim value from a JWT token.
    public string? GetClaimValue(string token, string claimType)
    {
        var claimsPrincipal = GetClaimsFromToken(token);
        var value = claimsPrincipal?.FindFirst(claimType)?.Value;
        return value;
    }

}
