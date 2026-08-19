using System.Net;
using Api.Exceptions;
using Api.Responses;
using Application.Auth;


namespace Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{

  private readonly RequestDelegate _next;
  private readonly ILogger<ExceptionHandlingMiddleware> _logger;

  public ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger
  ) {
    _next = next;
    _logger = logger;
  }

  public async Task InvokeAsync(HttpContext context) {
    try
    {
      await _next(context);
    }
    catch (BusinessException exception)
    {
      await WriteErrorAsync(
        context,
        HttpStatusCode.BadRequest,
        exception.Code,
        exception.Message
      );
    }
    catch (UnauthorizedAccessException)
    {
      await WriteErrorAsync(
        context,
        HttpStatusCode.Unauthorized,
        "Unauthorized",
        "请先登陆");
    }
    catch (InvalidCredentialsException exception)
    {
      await WriteErrorAsync(
        context,
        HttpStatusCode.BadRequest,
        "INVALID_CREDENTIALS",
        exception.Message);
    }
    catch (CurrentUserNotFoundException exception)
    {
      await WriteErrorAsync(
        context,
        HttpStatusCode.Unauthorized,
        "CURRENT_USER_NOTFOUND",
        exception.Message);
    } catch (Exception exception) {
      _logger.LogError(exception, "Unhandled exception");

      await WriteErrorAsync(
        context,
        HttpStatusCode.InternalServerError,
        "INTERNAL_SERVER_ERROR",
        "服务器开小差了，请稍后重试"
      );
    }
  }

  private static async Task WriteErrorAsync(
    HttpContext context,
    HttpStatusCode statusCode,
    string code,
    string message
  ) {
    if (context.Response.HasStarted) {
      throw new InvalidOperationException("Response has already started");
    }

    context.Response.Clear();
    context.Response.StatusCode = (int)statusCode;
    context.Response.ContentType = "application/json";
    
    var requestId = RequestIdProvider.Get(context);
    var response = ApiResponse<object>.Fail(code, message, requestId);

    await context.Response.WriteAsJsonAsync(response);
  }

}