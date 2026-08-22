namespace Application.Users;

public sealed class UpdateUserRequest
{
    public string? Email { get; init; }
    public string? Name { get; init; }
    public string? Password { get; init; }
    public string? Mobile { get; init; }
    public int? Role { get; init; }
    public int? Status { get; init; }
    public string? ManagerUserId { get; init; }
}