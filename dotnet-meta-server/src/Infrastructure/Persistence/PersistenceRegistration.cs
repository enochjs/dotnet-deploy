using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}