using Microsoft.Extensions.Options;

namespace Api.Configuration;

public static class OptionsRegistration
{
  public static IServiceCollection AddApplicationOptions(this IServiceCollection services)
  {
    services.AddValidatedOptions<PostgresOptions>(PostgresOptions.SectionName);
    services.AddValidatedOptions<RedisOptions>(RedisOptions.SectionName);
    services.AddValidatedOptions<GitOptions>(GitOptions.SectionName);
    services.AddValidatedOptions<DingTalkOptions>(DingTalkOptions.SectionName);
    services.AddValidatedOptions<OssOptions>(OssOptions.SectionName);
    services.AddValidatedOptions<InnerServerOptions>(InnerServerOptions.SectionName);
    services.AddValidatedOptions<MonitorOptions>(MonitorOptions.SectionName);
    services.AddValidatedOptions<LoggerOptions>(LoggerOptions.SectionName);
    services.AddValidatedOptions<JwtOptions>(JwtOptions.SectionName);
    return services;
  }

  private static OptionsBuilder<TOptions> AddValidatedOptions<TOptions>(this IServiceCollection services, string sectionName) where TOptions : class
  {
    return services
      .AddOptions<TOptions>()
      .BindConfiguration(sectionName)
      .ValidateDataAnnotations()
      .ValidateOnStart();
  }

}