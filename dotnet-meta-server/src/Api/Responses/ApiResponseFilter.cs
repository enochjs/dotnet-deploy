using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Responses;

public sealed class ApiResponseFilter: IAsyncResultFilter
{

  public async Task OnResultExecutionAsync(
    ResultExecutingContext context,
    ResultExecutionDelegate next
  ) {
    var statusCode = context.HttpContext.Response.StatusCode;

    if (
      context.Result is ObjectResult objectResult &&
      IsSuccessStatusCode(objectResult.StatusCode ?? statusCode) &&
      objectResult.Value is not null &&
      !IsApiResponse(objectResult.Value.GetType())
    ) {
      var requestId = RequestIdProvider.Get(context.HttpContext);
      var valueType = objectResult.Value.GetType();
      var responseType = typeof(ApiResponse<>).MakeGenericType(valueType);

      objectResult.Value = Activator.CreateInstance(
        responseType,
        true,
        "OK",
        "success",
        objectResult.Value,
        requestId
      );
    }

    await next();

  }

  private static bool IsSuccessStatusCode(int statusCode) {
    return statusCode >= 200 && statusCode < 300;
  }

  private static bool IsApiResponse(Type type) {
    return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>);
  }

}