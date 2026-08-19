using Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService authService, ICurrentUserAccessor currentUserAccessor): ControllerBase
{
  [HttpPost("login")]
  [AllowAnonymous]
  public Task<LoginResponse> Login(LoginRequest request, CancellationToken cancellationToken)
  {
    return authService.LoginAsync(request, cancellationToken);
  }

  [HttpGet("user")]
  [Authorize]
  public Task<AuthUserResponse> GetCurrentUser(CancellationToken cancellationToken)
  {
    var currentUser = currentUserAccessor.GetRequiredCurrentUser();
    return authService.GetCurrentUserAsync(currentUser, cancellationToken);
  }
}