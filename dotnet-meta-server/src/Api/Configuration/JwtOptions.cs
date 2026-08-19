using System.ComponentModel.DataAnnotations;

namespace Api.Configuration;

public sealed class JwtOptions
{
  public const string SectionName = "Jwt";

  [Required]
  public string Issuer { get; set; } = string.Empty;
  
  [Required]
  public string Audience { get; set; } = string.Empty;
  
  [Required]
  [MinLength(32)]
  public string SigningKey { get; set; } = string.Empty;
  
  [Range(5, 1440)]
  public int AccessTokenMinutes { get; set; }
}