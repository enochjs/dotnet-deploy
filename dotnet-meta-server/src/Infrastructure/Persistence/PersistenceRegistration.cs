using Application.Auth;
using Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Users;
using Infrastructure.Users;

namespace Infrastructure.Persistence;

public static class PersistenceRegistration
{
    public static IServiceCollection AddMetaServerPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("Postgres")["ConnectionString"];

        services.AddDbContext<MetaServerDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUserCredentialRepository, UserCredentialRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHashService, PasswordHashService>();

        return services;
    }
}