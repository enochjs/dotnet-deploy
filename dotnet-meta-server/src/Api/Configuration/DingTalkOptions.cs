using System.ComponentModel.DataAnnotations;

namespace Api.Configuration;

public sealed class DingTalkOptions
{
  public const string SectionName = "DingTalk";

  [Required]
  public string BaseUrl { get; init; } = string.Empty;

  [Required]
  public string AppKey { get; init; } = string.Empty;

  [Required]
  public string AppSecret { get; init; } = string.Empty;
}