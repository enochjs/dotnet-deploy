namespace Api.Responses;

public sealed record ApiResponse<T>(
  bool Success,
  string Code,
  string Message,
  T? Data,
  string RequestId
) {
  public static ApiResponse<T> Ok(T? data, string requestId) {
    return new ApiResponse<T>(true, "OK", "success", data, requestId);
  }

  public static ApiResponse<T> Fail(string code, string message, string requestId) {
    return new ApiResponse<T>(false, code, message, default, requestId);
  }
}
