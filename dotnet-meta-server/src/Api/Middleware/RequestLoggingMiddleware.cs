using System.Diagnostics;
using Api.Responses;
using Serilog.Context;

namespace Api.Middleware;

public sealed class RequestLoggingMiddleware
{

  private readonly RequestDelegate _next;
  private readonly ILogger<RequestLoggingMiddleware> _logger;

  public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger) {
    _next = next;
    _logger = logger;
  }

  public async Task InvokeAsync(HttpContext context) {
    var requestId = GetOrCreateRequestId(context);
    context.Items[RequestIdProvider.HeaderName] = requestId;
    context.Response.Headers[RequestIdProvider.HeaderName] = requestId;

    using var requestIdScope = LogContext.PushProperty("RequestId", requestId);

    var stopWatch = Stopwatch.StartNew();
    try {
      await _next(context);
    } finally {
      stopWatch.Stop();
      _logger.LogInformation(
        "Http {Method} {Path} responded {StatusCode} in {Elapsed}ms",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        stopWatch.ElapsedMilliseconds
      );
    }
  }

  private string GetOrCreateRequestId(HttpContext context) {
    if (
      context.Request.Headers.TryGetValue(RequestIdProvider.HeaderName, out var values)
    ) {
      var incomingRequestId = values.FirstOrDefault();
      if (!string.IsNullOrWhiteSpace(incomingRequestId)) {
        return incomingRequestId;
      }
    }
    return context.TraceIdentifier;
  }

}