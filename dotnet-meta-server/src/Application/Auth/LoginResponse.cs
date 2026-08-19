namespace Application.Auth;

public record LoginResponse(
  string AccessToken,
  DateTimeOffset ExpiresAt,
  CurrentUser User
);