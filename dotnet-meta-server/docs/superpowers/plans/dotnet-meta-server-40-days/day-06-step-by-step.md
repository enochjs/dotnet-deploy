# Day 06 Step-by-Step - User 模块与 LINQ CRUD

这份文档是 `day-06.md` 的跟做版。今天按一个正常后端开发的顺序实现 User 模块：先确认业务规则，再定义 DTO 和验证规则，然后写 Service、Repository、DI、Controller，最后补测试。

今天的新版方向很明确：

- 使用 Day 05 的标准本地登录和 JWT。
- 用户由当前系统自建和维护。
- 不再迁移 InnerServer 用户查询、第三方用户同步或权限中心 fallback。
- 搜索只查本地 `users` 表，找不到就返回空数组。
- 密码只保存 hash，不保存明文。

原 NestJS 项目只作为字段和接口形态参考；具体参考文件清单放在文档最后。

## 0. 今天最终要得到什么

完成后，你会新增这些能力：

- `POST /api/user/create`：创建本地用户，手机号唯一，密码写入 hash。
- `PUT /api/user/update/{id}`：更新用户基础信息，可选重置密码。
- `GET /api/user/detail/{id}`：查询用户详情。
- `GET /api/user/search?key=...`：按本地 `name`、`realName`、`userId` 搜索。
- `GET /api/user/list?pageIndex=1&pageSize=10&name=...`：分页查询。
- `UserService` 承担业务规则和 DTO 映射。
- `UserRepository` 用 EF Core LINQ 操作数据库。
- 集成测试覆盖重复手机号、用户不存在、本地搜索、分页。

最终验收：

- `dotnet build` 成功。
- `dotnet test` 成功。
- 新创建的用户可以用 Day 05 的 `/api/auth/login` 登录。

## 1. 回到项目根目录

### Step 1.1 进入 `dotnet-meta-server`

执行：

```bash
cd /Users/fenghe/enochjs/study/dotnet-deploy/dotnet-meta-server
```

确认当前位置：

```bash
pwd
```

应该看到：

```text
/Users/fenghe/enochjs/study/dotnet-deploy/dotnet-meta-server
```

### Step 1.2 确认 Day 05 代码能编译

执行：

```bash
dotnet build
dotnet test
```

如果这里失败，先修 Day 05。今天会新增完整业务模块，如果基础不干净，后面很难判断错误来自哪里。

## 2. 先确认业务规则

正常后端开发不会一上来写 Controller。先把业务边界写清楚：

```text
创建用户：
- name、mobile、password 必填
- mobile 必须是手机号格式
- mobile 唯一
- userId 默认等于 mobile
- password 保存为 PasswordHash
- role 默认 Other
- status 默认 Enabled

更新用户：
- id 从路由读取
- 用户不存在时报 USER_NOT_FOUND
- mobile 修改时仍然要校验唯一
- password 有值时重置密码 hash
- managerUserId 有值时只关联本地已有用户

详情：
- 按 id 查本地 users 表
- 找不到时报 USER_NOT_FOUND

搜索：
- 只查本地 users 表
- name / realName 使用模糊匹配
- userId 使用精确匹配
- 找不到返回空数组
- 不调用 InnerServer

分页：
- pageIndex 默认 1
- pageSize 默认 10
- pageSize 限制在 1 到 100
- 按 id 倒序
```

今天要练的 LINQ：

```csharp
AnyAsync
FirstOrDefaultAsync
CountAsync
Skip
Take
ToListAsync
```

## 3. 创建 DTO

### Step 3.1 创建目录

执行：

```bash
mkdir -p src/Application/Users
```

### Step 3.2 创建 `CreateUserRequest.cs`

创建文件：

```text
src/Application/Users/CreateUserRequest.cs
```

填入：

```csharp
namespace Application.Users;

public sealed class CreateUserRequest
{
    public string? Email { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Mobile { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public int? Role { get; init; }
    public int? Status { get; init; }
    public string? ManagerUserId { get; init; }
}
```

### Step 3.3 创建 `UpdateUserRequest.cs`

创建文件：

```text
src/Application/Users/UpdateUserRequest.cs
```

填入：

```csharp
namespace Application.Users;

public sealed class UpdateUserRequest
{
    public string? Email { get; init; }
    public string? Name { get; init; }
    public string? Mobile { get; init; }
    public string? Password { get; init; }
    public int? Role { get; init; }
    public int? Status { get; init; }
    public string? ManagerUserId { get; init; }
}
```

`id` 不放在 `UpdateUserRequest` 里，因为接口已经是 `PUT /api/user/update/{id}`。路由负责传 id，请求体只表达要更新的字段。

### Step 3.4 创建 `UserQueryRequest.cs`

创建文件：

```text
src/Application/Users/UserQueryRequest.cs
```

填入：

```csharp
namespace Application.Users;

public sealed class UserQueryRequest
{
    public int PageIndex { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Name { get; init; }
}
```

### Step 3.5 创建 `UserResponse.cs`

创建文件：

```text
src/Application/Users/UserResponse.cs
```

填入：

```csharp
namespace Application.Users;

public sealed record UserResponse(
    int Id,
    string UserId,
    string? DingTalkUserId,
    string? ManagerUserId,
    string? ManagerDingTalkUserId,
    string? Email,
    string Name,
    string? RealName,
    string Mobile,
    int Role,
    string RoleName,
    int Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
```

`RoleName` 是输出展示字段，不放进数据库实体。

## 4. 写验证规则

### Step 4.1 安装 FluentValidation

执行：

```bash
dotnet add src/Application/Application.csproj package FluentValidation.DependencyInjectionExtensions --version '12.*'
dotnet restore
```

这个包会提供：

```csharp
IValidator<T>
AddValidatorsFromAssemblyContaining<T>()
```

注意：`AddValidatorsFromAssemblyContaining<T>()` 只负责扫描并注册 validator，不会自动执行校验。真正执行校验的是后面 `UserService` 里的 `ValidateAsync`。

### Step 4.2 创建角色和状态常量

创建文件：

```text
src/Application/Users/UserRoles.cs
```

填入：

```csharp
namespace Application.Users;

public static class UserRoles
{
    public const int Frontend = 1;
    public const int Backend = 2;
    public const int Ued = 3;
    public const int Product = 4;
    public const int ProjectManager = 5;
    public const int Qa = 6;
    public const int Other = 99;

    public static bool IsValid(int? role)
    {
        return role is Frontend
            or Backend
            or Ued
            or Product
            or ProjectManager
            or Qa
            or Other;
    }

    public static string GetName(int role)
    {
        return role switch
        {
            Frontend => "前端",
            Backend => "后端",
            Ued => "UED",
            Product => "产品",
            ProjectManager => "项目经理",
            Qa => "测试",
            Other => "其他",
            _ => "未知",
        };
    }
}
```

创建文件：

```text
src/Application/Users/UserStatuses.cs
```

填入：

```csharp
namespace Application.Users;

public static class UserStatuses
{
    public const int Enabled = 1;
    public const int Disabled = 2;

    public static bool IsValid(int? status)
    {
        return status is Enabled or Disabled;
    }
}
```

### Step 4.3 创建 `CreateUserRequestValidator.cs`

创建文件：

```text
src/Application/Users/CreateUserRequestValidator.cs
```

填入：

```csharp
using FluentValidation;

namespace Application.Users;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(request => request.Mobile)
            .NotEmpty()
            .Matches("^1\\d{10}$")
            .WithMessage("手机号格式不正确");

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(64);

        RuleFor(request => request.Email)
            .MaximumLength(128)
            .EmailAddress()
            .When(request => !string.IsNullOrWhiteSpace(request.Email));

        RuleFor(request => request.Role)
            .Must(UserRoles.IsValid)
            .When(request => request.Role.HasValue)
            .WithMessage("角色不正确");

        RuleFor(request => request.Status)
            .Must(UserStatuses.IsValid)
            .When(request => request.Status.HasValue)
            .WithMessage("状态不正确");

        RuleFor(request => request.ManagerUserId)
            .MaximumLength(64)
            .When(request => !string.IsNullOrWhiteSpace(request.ManagerUserId));
    }
}
```

### Step 4.4 创建 `UpdateUserRequestValidator.cs`

创建文件：

```text
src/Application/Users/UpdateUserRequestValidator.cs
```

填入：

```csharp
using FluentValidation;

namespace Application.Users;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(request => request.Name)
            .MaximumLength(64)
            .When(request => request.Name is not null);

        RuleFor(request => request.Mobile)
            .Matches("^1\\d{10}$")
            .When(request => !string.IsNullOrWhiteSpace(request.Mobile))
            .WithMessage("手机号格式不正确");

        RuleFor(request => request.Password)
            .MinimumLength(6)
            .MaximumLength(64)
            .When(request => !string.IsNullOrWhiteSpace(request.Password));

        RuleFor(request => request.Email)
            .MaximumLength(128)
            .EmailAddress()
            .When(request => !string.IsNullOrWhiteSpace(request.Email));

        RuleFor(request => request.Role)
            .Must(UserRoles.IsValid)
            .When(request => request.Role.HasValue)
            .WithMessage("角色不正确");

        RuleFor(request => request.Status)
            .Must(UserStatuses.IsValid)
            .When(request => request.Status.HasValue)
            .WithMessage("状态不正确");

        RuleFor(request => request.ManagerUserId)
            .MaximumLength(64)
            .When(request => request.ManagerUserId is not null);
    }
}
```

## 5. 准备 Service 会用到的通用类型

Day 02 的 `Api.Exceptions.BusinessException` 和 `Api.Responses.PagedResponse<T>` 在 `Api` 层。`Application` 层不能反向引用 `Api`，所以这里创建应用层通用类型。

### Step 5.1 创建目录

执行：

```bash
mkdir -p src/Application/Common
```

### Step 5.2 创建 `BusinessRuleException.cs`

创建文件：

```text
src/Application/Common/BusinessRuleException.cs
```

填入：

```csharp
namespace Application.Common;

public sealed class BusinessRuleException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
```

### Step 5.3 创建 `RequestValidationException.cs`

创建文件：

```text
src/Application/Common/RequestValidationException.cs
```

填入：

```csharp
namespace Application.Common;

public sealed class RequestValidationException(
    IReadOnlyDictionary<string, string[]> errors
) : Exception("请求参数不正确")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
```

### Step 5.4 创建 `PagedResult.cs`

创建文件：

```text
src/Application/Common/PagedResult.cs
```

填入：

```csharp
namespace Application.Common;

public sealed record PagedResult<T>(
    int PageIndex,
    int PageSize,
    int TotalCount,
    IReadOnlyList<T> Items);
```

### Step 5.5 让异常中间件处理应用层异常

打开：

```text
src/Api/Middleware/ExceptionHandlingMiddleware.cs
```

增加 using：

```csharp
using Application.Common;
```

在现有 `catch (BusinessException exception)` 前面增加：

```csharp
catch (RequestValidationException exception)
{
    await WriteValidationErrorAsync(context, exception.Errors);
}
catch (BusinessRuleException exception)
{
    await WriteErrorAsync(
        context,
        HttpStatusCode.BadRequest,
        exception.Code,
        exception.Message);
}
```

再添加验证错误响应方法：

```csharp
private static async Task WriteValidationErrorAsync(
    HttpContext context,
    IReadOnlyDictionary<string, string[]> errors)
{
    if (context.Response.HasStarted)
    {
        throw new InvalidOperationException("Response has already started");
    }

    context.Response.Clear();
    context.Response.StatusCode = StatusCodes.Status400BadRequest;
    context.Response.ContentType = "application/json";

    var requestId = RequestIdProvider.Get(context);
    var response = ApiResponse<object>.Fail(
        "VALIDATION_ERROR",
        "请求参数不正确",
        requestId);

    await context.Response.WriteAsJsonAsync(new
    {
        response.Success,
        response.Code,
        response.Message,
        Data = errors,
        response.RequestId,
    });
}
```

到这里，中间件只负责“异常怎么变成统一响应”；它不会自动执行 FluentValidation。真正的校验调用会放在 `UserService`。

## 6. 定义 Service 依赖接口

### Step 6.1 创建 `IUserRepository.cs`

创建文件：

```text
src/Application/Users/IUserRepository.cs
```

填入：

```csharp
using Domain.Entities;

namespace Application.Users;

public interface IUserRepository
{
    Task<bool> ExistsByMobileOrUserIdAsync(
        string mobile,
        string userId,
        int? excludingId,
        CancellationToken cancellationToken);

    Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken);
    Task<User?> FindByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<User>> SearchLocalAsync(string key, CancellationToken cancellationToken);

    Task<(IReadOnlyList<User> Items, int TotalCount)> PageAsync(
        UserQueryRequest query,
        CancellationToken cancellationToken);

    void Add(User user);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
```

这里先定义接口，是因为 `UserService` 只关心业务需要什么能力，不关心 EF Core 怎么写 SQL。

## 7. 实现 UserService

### Step 7.1 创建 `UserService.cs`

创建文件：

```text
src/Application/Users/UserService.cs
```

填入：

```csharp
using Application.Auth;
using Application.Common;
using Domain.Entities;
using FluentValidation;

namespace Application.Users;

public sealed class UserService(
    IUserRepository users,
    IPasswordHashService passwordHashService,
    IValidator<CreateUserRequest> createValidator,
    IValidator<UpdateUserRequest> updateValidator)
{
    public async Task<UserResponse> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(createValidator, request, cancellationToken);

        var mobile = request.Mobile.Trim();
        var exists = await users.ExistsByMobileOrUserIdAsync(
            mobile,
            mobile,
            excludingId: null,
            cancellationToken);
        if (exists)
        {
            throw new BusinessRuleException("USER_MOBILE_EXISTS", "手机号已存在");
        }

        var now = DateTimeOffset.UtcNow;
        var user = new User
        {
            UserId = mobile,
            Email = NormalizeOptional(request.Email),
            Name = request.Name.Trim(),
            Mobile = mobile,
            ManagerUserId = NormalizeOptional(request.ManagerUserId),
            Role = request.Role ?? UserRoles.Other,
            Status = request.Status ?? UserStatuses.Enabled,
            CreatedAt = now,
            UpdatedAt = now,
        };
        user.PasswordHash = passwordHashService.HashPassword(user, request.Password);

        users.Add(user);
        await users.SaveChangesAsync(cancellationToken);

        return ToResponse(user);
    }

    public async Task<UserResponse> UpdateAsync(
        int id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(updateValidator, request, cancellationToken);

        var user = await users.FindByIdAsync(id, cancellationToken);
        if (user is null)
        {
            throw new BusinessRuleException("USER_NOT_FOUND", "用户不存在");
        }

        if (!string.IsNullOrWhiteSpace(request.Mobile))
        {
            var mobile = request.Mobile.Trim();
            var exists = await users.ExistsByMobileOrUserIdAsync(
                mobile,
                mobile,
                excludingId: id,
                cancellationToken);
            if (exists)
            {
                throw new BusinessRuleException("USER_MOBILE_EXISTS", "手机号已存在");
            }

            user.Mobile = mobile;
        }

        if (request.Email is not null)
        {
            user.Email = NormalizeOptional(request.Email);
        }

        if (request.Name is not null)
        {
            user.Name = request.Name.Trim();
        }

        if (request.Role.HasValue)
        {
            user.Role = request.Role.Value;
        }

        if (request.Status.HasValue)
        {
            user.Status = request.Status.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = passwordHashService.HashPassword(user, request.Password);
        }

        if (request.ManagerUserId is not null)
        {
            var managerUserId = NormalizeOptional(request.ManagerUserId);
            var manager = managerUserId is null
                ? null
                : await users.FindByUserIdAsync(managerUserId, cancellationToken);

            user.ManagerUserId = managerUserId;
            user.ManagerDingTalkUserId = manager?.DingTalkUserId;
        }

        user.UpdatedAt = DateTimeOffset.UtcNow;
        await users.SaveChangesAsync(cancellationToken);

        return ToResponse(user);
    }

    public async Task<UserResponse> GetDetailAsync(int id, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(id, cancellationToken);
        if (user is null)
        {
            throw new BusinessRuleException("USER_NOT_FOUND", "用户不存在");
        }

        return ToResponse(user);
    }

    public async Task<IReadOnlyList<UserResponse>> SearchAsync(
        string key,
        CancellationToken cancellationToken)
    {
        var normalizedKey = key.Trim();
        if (string.IsNullOrWhiteSpace(normalizedKey))
        {
            return [];
        }

        var localUsers = await users.SearchLocalAsync(normalizedKey, cancellationToken);
        return localUsers.Select(ToResponse).ToList();
    }

    public async Task<PagedResult<UserResponse>> PageAsync(
        UserQueryRequest query,
        CancellationToken cancellationToken)
    {
        var pageIndex = Math.Max(query.PageIndex, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var normalizedQuery = new UserQueryRequest
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Name = NormalizeOptional(query.Name),
        };

        var (items, totalCount) = await users.PageAsync(normalizedQuery, cancellationToken);

        return new PagedResult<UserResponse>(
            pageIndex,
            pageSize,
            totalCount,
            items.Select(ToResponse).ToList());
    }

    private static UserResponse ToResponse(User user)
    {
        return new UserResponse(
            user.Id,
            user.UserId,
            user.DingTalkUserId,
            user.ManagerUserId,
            user.ManagerDingTalkUserId,
            user.Email,
            user.Name,
            user.RealName,
            user.Mobile,
            user.Role,
            UserRoles.GetName(user.Role),
            user.Status,
            user.CreatedAt,
            user.UpdatedAt);
    }

    private static async Task ValidateAsync<T>(
        IValidator<T> validator,
        T request,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (result.IsValid)
        {
            return;
        }

        var errors = result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        throw new RequestValidationException(errors);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
```

这里的开发重点是：业务规则在 Service 里，Controller 不做业务判断，Repository 不做业务判断。

## 8. 实现 EF Core Repository

### Step 8.1 检查 User 实体和配置

确认 `src/Domain/Entities/User.cs` 有：

```csharp
public string PasswordHash { get; set; } = string.Empty;
```

确认 `src/Infrastructure/Persistence/Configurations/UserConfiguration.cs` 有：

```csharp
builder.Property(entity => entity.PasswordHash)
    .HasColumnName("password_hash")
    .HasMaxLength(512)
    .IsRequired();

builder.HasIndex(entity => entity.Mobile)
    .HasDatabaseName("ix_users_mobile")
    .IsUnique();

builder.HasIndex(entity => entity.UserId)
    .HasDatabaseName("ix_users_user_id")
    .IsUnique();
```

如果字段或索引不存在，先补齐。Day 05 已经处理过 `PasswordHash`，这里通常只是确认。

### Step 8.2 创建目录

执行：

```bash
mkdir -p src/Infrastructure/Users
```

### Step 8.3 创建 `UserRepository.cs`

创建文件：

```text
src/Infrastructure/Users/UserRepository.cs
```

填入：

```csharp
using Application.Users;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Users;

public sealed class UserRepository(MetaServerDbContext dbContext) : IUserRepository
{
    public Task<bool> ExistsByMobileOrUserIdAsync(
        string mobile,
        string userId,
        int? excludingId,
        CancellationToken cancellationToken)
    {
        return dbContext.Users
            .AnyAsync(
                user =>
                    (excludingId == null || user.Id != excludingId.Value)
                    && (user.Mobile == mobile || user.UserId == userId),
                cancellationToken);
    }

    public Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<User?> FindByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> SearchLocalAsync(
        string key,
        CancellationToken cancellationToken)
    {
        var pattern = $"%{key}%";

        return await dbContext.Users
            .Where(user =>
                EF.Functions.ILike(user.Name, pattern)
                || (user.RealName != null && EF.Functions.ILike(user.RealName, pattern))
                || user.UserId == key)
            .OrderByDescending(user => user.Id)
            .Take(20)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> PageAsync(
        UserQueryRequest query,
        CancellationToken cancellationToken)
    {
        var users = dbContext.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var pattern = $"%{query.Name.Trim()}%";
            users = users.Where(user => EF.Functions.ILike(user.Name, pattern));
        }

        var totalCount = await users.CountAsync(cancellationToken);
        var items = await users
            .OrderByDescending(user => user.Id)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(User user)
    {
        dbContext.Users.Add(user);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

这里用 `EF.Functions.ILike`，因为当前项目使用 PostgreSQL，它能生成大小写不敏感的模糊查询。

## 9. 注册 DI

### Step 9.1 注册 Application 服务和 validators

打开：

```text
src/Application/ApplicationRegistration.cs
```

改成：

```csharp
using Application.Auth;
using Application.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationRegistration
{
    public static IServiceCollection AddMetaServerApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();
        services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
        return services;
    }
}
```

这行：

```csharp
services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
```

只是把 validators 注册进 DI。它不会自动校验请求；校验发生在 `UserService` 的 `ValidateAsync`。

### Step 9.2 注册 Infrastructure 仓储

打开：

```text
src/Infrastructure/Persistence/PersistenceRegistration.cs
```

增加 using：

```csharp
using Application.Users;
using Infrastructure.Users;
```

在已有认证相关注册旁边增加：

```csharp
services.AddScoped<IUserRepository, UserRepository>();
```

关键片段类似：

```csharp
services.AddScoped<IUserCredentialRepository, UserCredentialRepository>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddSingleton<IPasswordHashService, PasswordHashService>();
```

## 10. 写 Controller

### Step 10.1 创建 `UserController.cs`

创建文件：

```text
src/Api/Controllers/UserController.cs
```

填入：

```csharp
using Application.Common;
using Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/user")]
public sealed class UserController(UserService userService) : ControllerBase
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
    public Task<UserResponse> Detail(int id, CancellationToken cancellationToken)
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
        CancellationToken cancellationToken)
    {
        return userService.PageAsync(query, cancellationToken);
    }
}
```

Controller 只负责 HTTP 路由和参数绑定。业务规则不要写在 Controller 里。

## 11. 明确不迁移 InnerServer

今天不要创建这些东西：

```text
IInnerServerUserClient
InnerServerUserInfo
Api/Integrations/InnerServer
FakeInnerServerUserClient
```

也不要在 `Program.cs` 里注册 InnerServer `HttpClient`。

新版用户来源只有本地 `users` 表。原 NestJS 的 `InnerServerService` 只是旧系统背景，不是今天要迁移的目标。

## 12. 先编译

执行：

```bash
dotnet build
```

常见问题：

- `IValidator<>` 找不到：检查 FluentValidation 包是否装在 `Application.csproj`。
- `PagedResult<>` 找不到：检查 `using Application.Common;`。
- `BusinessRuleException` 找不到：检查 `src/Application/Common` 文件和 using。
- `UserRepository` 找不到：检查 namespace 是否是 `Infrastructure.Users`。

## 13. 手动调用接口

### Step 13.1 启动 API

执行：

```bash
dotnet run --project src/Api/Api.csproj
```

记下本地地址，例如：

```text
http://localhost:5063
```

### Step 13.2 登录拿 token

执行：

```bash
curl -s -X POST http://localhost:5063/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"account":"13800000001","password":"123456"}'
```

从 `data.accessToken` 复制 token。

### Step 13.3 创建用户

执行：

```bash
curl -s -X POST http://localhost:5063/api/user/create \
  -H 'Content-Type: application/json' \
  -H 'Authorization: Bearer <accessToken>' \
  -d '{"name":"new-dev","mobile":"13900000001","password":"123456","email":"new-dev@example.com","role":2,"status":1}'
```

验收：

- 返回新用户。
- `userId` 默认等于手机号。
- `roleName` 是 `后端`。
- 响应里不会返回 `password` 或 `passwordHash`。

再用新用户登录：

```bash
curl -s -X POST http://localhost:5063/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"account":"13900000001","password":"123456"}'
```

能拿到 token，说明自建用户已经接入标准登录。

### Step 13.4 测重复手机号

再次执行创建用户命令。

验收：

- HTTP 状态是 400。
- `code` 是 `USER_MOBILE_EXISTS`。
- `message` 是 `手机号已存在`。

### Step 13.5 查分页

执行：

```bash
curl -s 'http://localhost:5063/api/user/list?pageIndex=1&pageSize=10' \
  -H 'Authorization: Bearer <accessToken>'
```

验收：

- `data.pageIndex` 是 1。
- `data.pageSize` 是 10。
- `data.items` 有用户数据。

### Step 13.6 搜索本地用户

执行：

```bash
curl -s 'http://localhost:5063/api/user/search?key=dev' \
  -H 'Authorization: Bearer <accessToken>'
```

验收：

- 本地命中时返回用户数组。
- 本地不命中时返回空数组。
- 不调用外部系统。

## 14. 写集成测试

### Step 14.1 创建 `UserApiTests.cs`

创建文件：

```text
tests/IntegrationTests/UserApiTests.cs
```

填入：

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.Support;

namespace IntegrationTests;

public sealed class UserApiTests : IClassFixture<TestEnvironmentFixture>
{
    private readonly TestEnvironmentFixture _fixture;

    public UserApiTests(TestEnvironmentFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Create_WithDuplicateMobile_ReturnsBusinessError()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/user/create",
            new { name = "duplicate", mobile = "13800000001", password = "123456" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());

        Assert.Equal("USER_MOBILE_EXISTS", json.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Detail_WithMissingUser_ReturnsUserNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/user/detail/999999");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());

        Assert.Equal("USER_NOT_FOUND", json.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Search_WithLocalMatch_ReturnsUsers()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/user/search?key=owner");

        response.EnsureSuccessStatusCode();

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        var items = json.RootElement.GetProperty("data").EnumerateArray().ToList();

        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task Search_WithMissingLocalMatch_ReturnsEmptyArray()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/user/search?key=not-exists-user");

        response.EnsureSuccessStatusCode();

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        var items = json.RootElement.GetProperty("data").EnumerateArray().ToList();

        Assert.Empty(items);
    }

    [Fact]
    public async Task List_ReturnsPagedUsersOrderedByIdDescending()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/user/list?pageIndex=1&pageSize=1");

        response.EnsureSuccessStatusCode();

        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        var data = json.RootElement.GetProperty("data");

        Assert.Equal(1, data.GetProperty("pageIndex").GetInt32());
        Assert.Equal(1, data.GetProperty("pageSize").GetInt32());
        Assert.True(data.GetProperty("totalCount").GetInt32() >= 2);
        Assert.Single(data.GetProperty("items").EnumerateArray());
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _fixture.Factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { account = "13800000001", password = "123456" });
        loginResponse.EnsureSuccessStatusCode();

        using var loginJson = await JsonDocument.ParseAsync(
            await loginResponse.Content.ReadAsStreamAsync());

        var accessToken = loginJson.RootElement
            .GetProperty("data")
            .GetProperty("accessToken")
            .GetString();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        return client;
    }
}
```

测试放在最后，是为了先把业务实现跑通，再用测试固化行为。真实项目里也可以先写测试再实现；但学习文档里先讲清概念和业务层次更重要。

## 15. 运行测试

执行：

```bash
dotnet test
```

验收：

- Auth、Diagnostics、Database、Redis、User 相关测试全部通过。
- 测试输出中能看到 Testcontainers 启动 PostgreSQL 和 Redis。

如果搜索测试失败，优先检查：

- `UserRepository.SearchLocalAsync` 是否查询了 `Name`、`RealName` 和 `UserId`。
- 本地无命中时是否返回空数组。
- 是否误加了 InnerServer 或外部同步逻辑。

如果重复手机号测试失败，优先检查：

- `CreateAsync` 是否先 `Trim()` 手机号。
- `ExistsByMobileOrUserIdAsync` 是否同时检查 `Mobile` 和 `UserId`。
- 种子数据里是否已经有 `13800000001`。

## 16. 今天的学习检查

完成后，确认你能解释：

- 为什么先写 DTO，再写 Controller？
- `AddValidatorsFromAssemblyContaining` 为什么只是注册，不是执行？
- 为什么真正的 `ValidateAsync` 放在 `UserService`？
- 为什么 `Application` 层不引用 `Api` 层类型？
- 为什么 Controller 不直接注入 `MetaServerDbContext`？
- `AnyAsync`、`FirstOrDefaultAsync`、`CountAsync` 分别适合什么场景？
- 为什么新版用户搜索不调用 InnerServer？

## 17. 今日验收清单

- [ ] 业务规则已经明确：本地用户、本地登录、本地搜索。
- [ ] `Application/Users` 中有 request DTO、response DTO、validator、常量、仓储接口和 `UserService`。
- [ ] `Application/Common` 中有业务异常、验证异常和分页结果。
- [ ] `ExceptionHandlingMiddleware` 能处理应用层异常。
- [ ] `Infrastructure/Users/UserRepository.cs` 用 EF Core LINQ 实现查询。
- [ ] DI 注册了 `UserService`、validators、`IUserRepository`。
- [ ] `Api/Controllers/UserController.cs` 提供 5 个接口。
- [ ] 没有创建 InnerServer、第三方用户查询或外部用户同步逻辑。
- [ ] `dotnet build` 通过。
- [ ] `dotnet test` 通过。

## 18. 晚上复盘

可以按这个格式记录：

```text
今天学会的 C#/.NET 概念：
-

今天完成的工程产物：
-

今天最容易踩坑的点：
-

明天风险：
-
```

## 参考源码与资料

原 NestJS 功能参考：

- `/Users/fenghe/workspace/devops/meta-server/src/biz/user/user.controller.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/biz/user/user.service.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/biz/user/dto/opreate.dto.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/biz/user/dto/user.query.dto.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/biz/user/dto/user.dto.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/core/entities/user.entity.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/constants/user.ts`

当前 .NET 项目风格参考：

- `docs/superpowers/plans/dotnet-meta-server-40-days/day-02-step-by-step.md`
- `docs/superpowers/plans/dotnet-meta-server-40-days/day-04-step-by-step.md`
- `docs/superpowers/plans/dotnet-meta-server-40-days/day-05-step-by-step.md`
- `src/Api/Controllers/AuthController.cs`
- `src/Application/Auth/AuthService.cs`
- `src/Infrastructure/Persistence/MetaServerDbContext.cs`
- `tests/IntegrationTests/AuthApiTests.cs`

外部资料：

- FluentValidation ASP.NET Core 文档：https://docs.fluentvalidation.net/en/latest/aspnet.html
- FluentValidation 依赖注入文档：https://docs.fluentvalidation.net/en/latest/di.html
- FluentValidation 异步验证文档：https://docs.fluentvalidation.net/en/latest/async.html
