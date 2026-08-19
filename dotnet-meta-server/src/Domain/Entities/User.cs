namespace Domain.Entities;

public sealed class User
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? DingTalkUserId { get; set; }
    public string? ManagerUserId { get; set; }
    public string? ManagerDingTalkUserId { get; set; }
    public string? Email { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? RealName { get; set; }
    public string Mobile { get; set; } = string.Empty;
    public int Role { get; set; }
    public int Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}