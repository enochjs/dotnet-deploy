using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Configuration;
using Application.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> jwtOptions): IJwtTokenService
{

  private readonly JwtOptions _options = jwtOptions.Value;

  public LoginResponse CreateAccessToken(CurrentUser user)
  {
    var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenMinutes);
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
      new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
      new Claim(JwtClaimTypes.UserId, user.UserId),
      new Claim(JwtClaimTypes.Mobile, user.Mobile),
      new Claim(JwtClaimTypes.Name, user.Name),
      new Claim(JwtClaimTypes.Role, user.Role.ToString()),
    };

    var token = new JwtSecurityToken(
      issuer: _options.Issuer,
      audience: _options.Audience,
      claims: claims,
      notBefore: DateTime.UtcNow,
      expires: expiresAt.UtcDateTime,
      signingCredentials: credentials
    );
    var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
    return new LoginResponse(accessToken, expiresAt, user);
  }
}