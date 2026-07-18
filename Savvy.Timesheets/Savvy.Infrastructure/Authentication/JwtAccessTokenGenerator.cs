using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Savvy.Application.Authentication;
using Savvy.Domain;

namespace Savvy.Infrastructure.Authentication;

public sealed class JwtAccessTokenGenerator(IOptions<JwtOptions> options) : IAccessTokenGenerator
{
    private readonly JwtOptions _options = options.Value;

    /// <summary>Creates a signed JWT containing the user's identity, role, and optional practice scope.</summary>
    public AccessToken Generate(User user)
    {
        var expiresUtc = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
        };

        if (user.PracticeId.HasValue)
        {
            claims.Add(new Claim("practice_id", user.PracticeId.Value.ToString()));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            expires: expiresUtc,
            signingCredentials: credentials
        );

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresUtc);
    }
}
