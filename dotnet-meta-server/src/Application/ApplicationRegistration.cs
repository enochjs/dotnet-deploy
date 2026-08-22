

using Application.Auth;
using Application.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationRegistration
{
  public static IServiceCollection AddMetaServerApplication(this IServiceCollection services)
  {
    services.AddScoped<AuthService>();
    services.AddScoped<UserService>();
    services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
    return services;
  }
}