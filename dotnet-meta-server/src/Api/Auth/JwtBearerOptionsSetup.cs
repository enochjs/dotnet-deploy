using System.Text;
using Api.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Auth;

public sealed class JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions): IConfigureNamedOptions<JwtBearerOptions>
{
  private readonly JwtOptions _jwtOptions = jwtOptions.Value;

  public void Configure(string? name, JwtBearerOptions options)
  {
    if (name != JwtBearerDefaults.AuthenticationScheme)
    {
      return;
    }
    
    Configure(options);
  }

  public void Configure(JwtBearerOptions options)
  {
    options.RequireHttpsMetadata = false;
    options.SaveToken = false;
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidIssuer = _jwtOptions.Issuer,
      ValidateAudience = true,
      ValidAudience = _jwtOptions.Audience,
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes((_jwtOptions.SigningKey))),
      ValidateLifetime = true,
      ClockSkew = TimeSpan.FromMinutes(1)
    };
  }
  
}