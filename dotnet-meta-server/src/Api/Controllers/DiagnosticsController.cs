using System.ComponentModel.DataAnnotations;
using Api.Exceptions;
using Api.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Api.Controllers;

[ApiController]
[Route("api/diagnostics")]
public sealed class DiagnosticsController: ControllerBase {
  [HttpGet("success")]
  public ActionResult<object> Success() {
    return Ok(new {
      status = "ok"
    });
  }

  [HttpGet("business-error")]
  public ActionResult BusinessError() {
    throw new BusinessException("DEMO_BUSINESS_ERROR", "这是一个业务异常示例");
  }

  [HttpGet("server-error")]
  public ActionResult ServerError() {
    throw new InvalidOperationException("This is a demo exception.");
  }

  [Authorize]
  [HttpGet("secure")]
  public ActionResult<object> Secure() {
    return Ok(new {
      status = "secure"
    });
  }

  [HttpGet("paged")]
  public ActionResult<PagedResponse<string>> Paged() {
    return Ok(new PagedResponse<string>(
      PageIndex: 1,
      PageSize: 10,
      TotalCount: 100,
      Items: ["alpha", "beta"]
    ));
  }

  [HttpPost("validation")]
  public ActionResult<object> Validation(ValidationRequest request) {
    return Ok(new {
      request.Name
    });
  }
}

public sealed record ValidationRequest(
  [Required] string Name
);