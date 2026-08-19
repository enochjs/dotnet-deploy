namespace Application.Auth;

public interface IJwtTokenService
{
  LoginResponse CreateAccessToken(CurrentUser user);
}