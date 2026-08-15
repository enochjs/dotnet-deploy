using System.ComponentModel.DataAnnotations;

namespace Api.Configuration;

public sealed class OssOptions
{
  public const string SectionName = "Oss";

  [Required]
  public string Endpoint { get; init; } = string.Empty;

  [Required]
  public string BucketName { get; init; } = string.Empty;

  public string AccessKeyId { get; init; } = string.Empty;

  public string AccessKeySecret { get; init; } = string.Empty;
}