using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Api.Responses;

public sealed class AuthorizationResultHandler: IAuthorizationMiddlewareResultHandler
{

  private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();


  public async Task HandleAsync(
    RequestDelegate next,
    HttpContext context,
    AuthorizationPolicy policy,
    PolicyAuthorizationResult authorizeResult
  ) {
    if (authorizeResult.Challenged) {

      context.Response.StatusCode = StatusCodes.Status401Unauthorized;
      context.Response.ContentType = "application/json";

      var response = ApiResponse<object>.Fail(
        "UNAUTHORIZED",
        "请先登录",
        RequestIdProvider.Get(context)
      );

      await context.Response.WriteAsJsonAsync(response);
      return;

    }

    await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
  }

}