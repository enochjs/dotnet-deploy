namespace Api.Responses;

public sealed record PagedResponse<T>(
  int PageIndex,
  int PageSize,
  int TotalCount,
  IReadOnlyList<T> Items
);