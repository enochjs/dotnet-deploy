namespace Application.Auth;

public record AuthUserResponse(
  int Id,
  string UserId,
  string? Email,
  string Name,
  string? RealName,
  string Mobile,
  int Role,
  int Status,
  DateTimeOffset CreatedAt,
  DateTimeOffset UpdatedAt);