using System.ComponentModel.DataAnnotations;

namespace Application.Auth;

public sealed class LoginRequest
{
  [Required]
  public string Account { get; set; } = string.Empty;
  
  [Required]
  public string Password { get; set; } = string.Empty;
}