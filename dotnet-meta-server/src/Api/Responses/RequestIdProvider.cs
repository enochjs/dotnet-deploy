namespace Api.Responses;

public static class RequestIdProvider {

  public const string HeaderName = "X-Request-Id";

  public static string Get(HttpContext context)
  {
    if (context.Items.TryGetValue(HeaderName, out var value) && 
      value is string requestId &&
      !string.IsNullOrWhiteSpace(requestId)
    ) {
      return requestId;
    }
    return context.TraceIdentifier;
  }
}