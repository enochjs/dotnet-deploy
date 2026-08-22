namespace Application.Users;

public sealed class CreateUserRequest
{
    public string? Email { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Mobile { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public int? Role { get; init; }
    public int? Status { get; init; }
    public string? ManagerUserId { get; init; }
}