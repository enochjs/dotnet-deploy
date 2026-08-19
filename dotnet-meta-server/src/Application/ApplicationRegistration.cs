

using Application.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationRegistration
{
  public static IServiceCollection AddMetaServerApplication(this IServiceCollection services)
  {
    services.AddScoped<AuthService>();
    return services;
  }
}