using Application.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Api.Auth;

public static class AuthenticationRegistration
{
  public static IServiceCollection AddMetaServerAuthentication(this IServiceCollection services)
  {
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
    services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();
    services.AddScoped<IJwtTokenService, JwtTokenService>();
    services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
    
    return services;
  }
}