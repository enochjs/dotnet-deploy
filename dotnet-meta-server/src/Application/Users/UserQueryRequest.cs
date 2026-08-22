namespace Application.Users;

public sealed class UserQueryRequest
{
    public int PageIndex { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Name { get; init; }
}