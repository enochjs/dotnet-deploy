namespace Application.Auth;

public sealed record CurrentUser(
  int Id,
  string UserId,
  string Mobile,
  string Name,
  int Role
);