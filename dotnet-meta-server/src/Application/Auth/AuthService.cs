using System.Security.Authentication;
using Domain.Entities;

namespace Application.Auth;

public class AuthService(
  IUserCredentialRepository users,
  IPasswordHashService passwordHashService,
  IJwtTokenService jwtTokenService
)
{
  private readonly IUserCredentialRepository _users = users;
  private readonly IPasswordHashService _passwordHashService = passwordHashService;
  private readonly IJwtTokenService _jwtTokenService = jwtTokenService;

  public async Task<LoginResponse> LoginAsync(
    LoginRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = request.Account.Trim();
    var user = await _users.FindByAccountAsync(account, cancellationToken);
    if (user is null || !_passwordHashService.VerifyPassword(user, request.Password))
    {
      throw new InvalidCredentialException();
    }
    return _jwtTokenService.CreateAccessToken(ToCurrentUser(user));
  }


  public async Task<AuthUserResponse> GetCurrentUserAsync(CurrentUser currentUser, CancellationToken cancellationToken)
  {
    var user = await _users.FindByIdAsync(currentUser.Id, cancellationToken);
    if (user is null)
    {
      throw new CurrentUserNotFoundException();
    }
    return ToResponse(user);
  }

  private static CurrentUser ToCurrentUser(User user)
  {
    return new CurrentUser(
      user.Id,
      user.UserId,
      user.Mobile,
      user.Name,
      user.Role
    );
  }

  private static AuthUserResponse ToResponse(User user)
  {
    return new AuthUserResponse(
      user.Id,
      user.UserId,
      user.Email,
      user.Name,
      user.RealName,
      user.Mobile,
      user.Role,
      user.Status,
      user.CreatedAt,
      user.UpdatedAt);
  }
  
}
