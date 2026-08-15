using System.ComponentModel.DataAnnotations;

namespace Api.Configuration;

public sealed class InnerServerOptions
{
  public const string SectionName = "InnerServer";

  [Required]
  public string BaseUrl { get; init; } = string.Empty;

  public int TimeoutSeconds { get; init; } = 30;
}