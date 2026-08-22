using Application.Common;
using Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/user")]
public sealed class UserController(UserService userService): ControllerBase
{
    [HttpPost("create")]
    public Task<UserResponse> Create(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        return userService.CreateAsync(request, cancellationToken);
    }
    
    [HttpPut("update/{id:int}")]
    public Task<UserResponse> Update(
        int id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        return userService.UpdateAsync(id, request, cancellationToken);
    }

    [HttpGet("detail/{id:int}")]
    public Task<UserResponse> Detail(
        int id,
        CancellationToken cancellationToken
    )
    {
        return userService.GetDetailAsync(id, cancellationToken);
    }

    [HttpGet("search")]
    public Task<IReadOnlyList<UserResponse>> Search(
        [FromQuery] string key,
        CancellationToken cancellationToken)
    {
        return userService.SearchAsync(key, cancellationToken);
    }

    [HttpGet("list")]
    public Task<PagedResult<UserResponse>> List(
        [FromQuery] UserQueryRequest query,
        CancellationToken cancellationToken
    )
    {
        return userService.PageAsync(query, cancellationToken);
    }
}