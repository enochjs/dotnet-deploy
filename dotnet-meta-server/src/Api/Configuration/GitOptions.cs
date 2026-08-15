using System.ComponentModel.DataAnnotations;

namespace Api.Configuration;

public sealed class GitOptions
{
  public const string SectionName = "Git";

  [Required]
  public string Url { get; init; } = string.Empty;

  public string DefaultBranch { get; init; } = "main";
}