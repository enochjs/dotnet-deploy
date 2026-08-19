using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Application.Auth;

namespace Api.Auth;

public sealed class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor): ICurrentUserAccessor

{
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public CurrentUser GetRequiredCurrentUser()
  {
    var principal = _httpContextAccessor.HttpContext?.User;

    if (principal?.Identity?.IsAuthenticated != true)
    {
      throw new UnauthorizedAccessException("当前请求未登陆");
    }
    
    var id = ReadRequiredIntClaim(principal, JwtRegisteredClaimNames.Sub);
    var userId = ReadRequiredStringClaim(principal, JwtClaimTypes.UserId);
    var mobile = ReadRequiredStringClaim(principal, JwtClaimTypes.Mobile);
    var name = ReadRequiredStringClaim(principal, JwtClaimTypes.Name);
    var role = ReadRequiredIntClaim(principal, JwtClaimTypes.Role);
    
    return new CurrentUser(id, userId, mobile, name, role);
  }

  private static string ReadRequiredStringClaim(ClaimsPrincipal principal, string claimType)
  {
    var value = principal.FindFirstValue(claimType);
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new UnauthorizedAccessException($"缺少用户身份字段：{claimType}");
    }
    return value;
  }

  private static int ReadRequiredIntClaim(ClaimsPrincipal principal, string claimType)
  {
    var value = ReadRequiredStringClaim(principal, claimType);
    if (!int.TryParse(value, out var result))
    {
      throw new UnauthorizedAccessException($"用户身份字段格式不正确：{claimType}");
    }

    return result;
  }
}
