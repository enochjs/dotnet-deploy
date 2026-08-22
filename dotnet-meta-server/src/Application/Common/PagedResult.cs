namespace Application.Common;

public sealed record PagedResult<T>(
    int PageIndex,
    int PageSize,
    int TotalCount,
    IReadOnlyList<T>  Items);