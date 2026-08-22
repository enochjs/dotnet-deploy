namespace Application.Users;

public sealed record UserResponse(
    int Id,
    string? UserId,
    string? DingTalkUserId,
    string? ManagerUserId,
    string? ManagerDingTalkUserId,
    string? Email,
    string Name,
    string? RealName,
    string Mobile,
    int Role,
    string RoleName,
    int Status,
    DateTimeOffset CreateAt,
    DateTimeOffset? UpdateAt);