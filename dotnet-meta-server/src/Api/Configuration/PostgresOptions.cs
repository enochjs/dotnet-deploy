using System.ComponentModel.DataAnnotations;

namespace Api.Configuration;

public sealed class PostgresOptions
{

  public const string SectionName = "Postgres";

  [Required]
  public string ConnectionString { get; init; } = string.Empty;
}