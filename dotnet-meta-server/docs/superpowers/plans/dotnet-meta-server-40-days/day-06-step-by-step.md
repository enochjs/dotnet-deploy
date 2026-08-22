# Day 06 Step-by-Step - User 模块与 LINQ CRUD

这份文档是 `day-06.md` 的跟做版。今天会把后端从“可以登录并读取当前用户”，推进到“有一个完整的业务 CRUD 模块”：用户创建、更新、详情、搜索和分页。

先明确今天的边界：

- Day 05 已经完成本地登录、JWT 鉴权和 `PasswordHash`，今天不重复做登录。
- 原 NestJS 项目的用户模块是功能参考，不照搬 TypeORM 写法。
- 新版 meta-server 使用标准本地登录，用户也由本系统自建和维护，不再调用 InnerServer 或其他第三方接口同步用户。
- 当前 .NET 项目继续保持分层：`Api` 负责 HTTP，`Application` 负责业务编排，`Infrastructure` 负责 EF Core LINQ 和数据库访问。
- FluentValidation 今天使用显式调用 `ValidateAsync` 的方式，不接入旧式 MVC 自动验证管线。这样规则以后可以安全扩展为异步校验。

原业务源码只作为功能参考；具体参考文件清单放在文档最后。

## 0. 今天最终要得到什么

完成后，你会在 Day 05 的项目上新增这些能力：

- `Application/Users` 中有用户请求 DTO、返回 DTO、验证器、业务服务和仓储接口。
- `Infrastructure` 中有基于 `MetaServerDbContext` 的 `UserRepository`，用 EF Core LINQ 实现查询、分页、创建、更新。
- `Api` 中有 `UserController`，对齐原接口：
  - `POST /api/user/create`
  - `PUT /api/user/update/{id}`
  - `GET /api/user/detail/{id}`
  - `GET /api/user/search?key=...`
  - `GET /api/user/list?pageIndex=1&pageSize=10&name=...`
- 创建用户时校验手机号唯一。
- 更新用户时可以更新 manager 信息。
- 搜索用户时只查本地 `users` 表，本地没有命中就返回空数组。
- 分页返回继续使用 Day 02 建立的分页结构：`pageIndex`、`pageSize`、`totalCount`、`items`。
- 集成测试覆盖重复手机号、用户不存在、本地搜索和分页。

你最后需要确认三件事：

- `dotnet build` 成功。
- `dotnet test` 成功。
- 登录后携带 JWT 访问 `/api/user/list` 能看到统一响应结构和分页数据。

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

你应该看到：

```text
/Users/fenghe/enochjs/study/dotnet-deploy/dotnet-meta-server
```

### Step 1.2 确认 Day 05 代码能编译

执行：

```bash
dotnet build
dotnet test
```

验收：

- `dotnet build` 能看到 `Build succeeded.`
- `dotnet test` 能看到 `Passed!`

如果这里失败，先修 Day 05。今天会新增 Controller、Service、Repository 和集成测试，如果基础不干净，后面很难判断错误来自哪里。

## 2. 先理解今天的用户模块流程

### Step 2.1 对比原 NestJS 用户模块

原项目里用户模块的主要职责是：

```text
UserController
        |
UserService
        |
TypeORM Repository<User>
        |
users 表
```

搜索用户只走本地路径：

```text
GET /api/user/search?key=...
        |
先查本地 users 表：name like、realName like、userId exact
        |
返回本地命中的用户；没有命中就返回空数组
```

今天在 .NET 中保持同样的业务意图，但拆分方式会更清楚：

```text
UserController
        |
UserService
        |
IUserRepository
        |
EF Core LINQ
        |
PostgreSQL users 表
```

### Step 2.2 记住今天要练的 LINQ

今天会反复看到这些查询：

```csharp
await query.AnyAsync(cancellationToken);
await query.FirstOrDefaultAsync(cancellationToken);
await query.CountAsync(cancellationToken);
await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
```

你可以先这样理解：

- `AnyAsync`：判断是否存在，适合做唯一性校验。
- `FirstOrDefaultAsync`：查询一个对象，找不到返回 `null`。
- `CountAsync`：分页前算总数。
- `Skip` + `Take`：跳过前面几页，再取当前页。

## 3. 安装 FluentValidation

### Step 3.1 给 Application 添加 FluentValidation

执行：

```bash
dotnet add src/Application/Application.csproj package FluentValidation.DependencyInjectionExtensions --version '12.*'
dotnet restore
```

为什么装在 `Application`：

- 请求 DTO 和验证规则属于应用业务入口。
- `Application` 不依赖 ASP.NET Core，也不依赖 EF Core。
- Controller 和测试都可以复用同一套验证规则。

今天不安装 `FluentValidation.AspNetCore`，也不启用旧式自动验证。我们会在 `UserService` 中显式调用 `ValidateAsync`，这样规则里未来出现数据库唯一性等异步逻辑时，不会被 MVC 同步验证管线卡住。

## 4. 创建应用层通用异常

Day 02 的 `Api.Exceptions.BusinessException` 放在 `Api` 层。今天的用户业务服务在 `Application` 层，不能反过来引用 `Api`。所以先在 `Application` 层创建自己的业务异常，再让 `Api` 中间件识别它。

### Step 4.1 创建目录

执行：

```bash
mkdir -p src/Application/Common
```

### Step 4.2 创建 `BusinessRuleException.cs`

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

### Step 4.3 创建 `RequestValidationException.cs`

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

`BusinessRuleException` 表示“参数格式没错，但业务规则不允许”，例如手机号重复。`RequestValidationException` 表示“请求本身不合法”，例如手机号格式错误。

### Step 4.4 修改异常中间件

打开：

```text
src/Api/Middleware/ExceptionHandlingMiddleware.cs
```

增加 using：

```csharp
using Application.Common;
```

在 `catch (BusinessException exception)` 前面增加两个分支：

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

再在 `WriteErrorAsync` 下面增加：

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

验收：

- 编译器不应该提示 `Application.Common` 找不到。
- 中间件仍然保留 Day 02 已有的异常处理分支。

### Step 4.5 创建应用层分页结果

Day 02 的 `Api.Responses.PagedResponse<T>` 放在 `Api` 层。今天用户服务在 `Application` 层，不能反向引用 API 表现层类型，所以创建一个应用层分页结果：

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

统一响应包装仍然由 Day 02 的 `ApiResponseFilter` 处理。Controller 返回 `PagedResult<T>` 后，前端看到的 `data` 结构仍然是熟悉的分页字段。

## 5. 创建用户 DTO 和验证器

### Step 5.1 创建目录

执行：

```bash
mkdir -p src/Application/Users
```

### Step 5.2 创建 `CreateUserRequest.cs`

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

### Step 5.3 创建 `UpdateUserRequest.cs`

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

原 NestJS 的 `UpdateUserDto` 里有 `id` 字段，但接口本身已经是 `PUT /api/user/update/:id`。在 ASP.NET Core 里今天只从路由读 `id`，请求体不再重复传 `id`。

### Step 5.4 创建 `UserQueryRequest.cs`

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

### Step 5.5 创建 `UserResponse.cs`

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

`RoleName` 对应原 NestJS 实体里的 `roleName()`。这里不把它放进领域实体，是因为角色文案更像输出格式，不是数据库核心状态。

### Step 5.6 创建验证器

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

### Step 5.7 创建角色和状态常量

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

这里先用 `int` 常量，不急着改领域实体为 enum。数据库里已经是 int，今天的核心是 CRUD 和 LINQ，不把变更扩大到全项目枚举重构。

## 6. 定义用户仓储接口

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

`excludingId` 用在更新场景：用户改手机号时，要排除自己这条记录，否则会误判“自己的手机号已存在”。

今天不再定义 InnerServer 接口。用户来源只有本系统的 `users` 表，搜索找不到时返回空数组，不向外部系统补查。

## 7. 实现用户业务服务

### Step 7.1 创建 `UserService.cs`

创建文件：

```text
src/Application/Users/UserService.cs
```

填入：

```csharp
using Application.Auth;
using Application.Common;
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

创建用户时必须通过 `IPasswordHashService` 写入 `PasswordHash`，不要保存明文密码，也不要给新用户写固定默认密码。后续如果要做“管理员重置密码”，可以复用 `UpdateAsync` 里的可选 `Password` 更新逻辑。

## 8. 注册应用层服务和验证器

### Step 8.1 修改 `ApplicationRegistration.cs`

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

`AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>()` 会扫描 `Application` 程序集里的所有 FluentValidation 验证器，并注册为 `IValidator<T>`。

## 9. 用 EF Core LINQ 实现 UserRepository

### Step 9.1 创建目录

执行：

```bash
mkdir -p src/Infrastructure/Users
```

### Step 9.2 创建 `UserRepository.cs`

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

这里用 `EF.Functions.ILike` 而不是 `string.Contains`，是因为 PostgreSQL 的 `ILIKE` 可以做大小写不敏感匹配，生成的 SQL 更接近我们真正想表达的查询。今天项目使用 Npgsql，所以可以使用这个 PostgreSQL 扩展。

### Step 9.3 注册仓储

打开：

```text
src/Infrastructure/Persistence/PersistenceRegistration.cs
```

增加 using：

```csharp
using Application.Users;
using Infrastructure.Users;
```

在已有用户认证仓储注册下面增加：

```csharp
services.AddScoped<IUserRepository, UserRepository>();
```

最终关键片段类似：

```csharp
services.AddScoped<IUserCredentialRepository, UserCredentialRepository>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddSingleton<IPasswordHashService, PasswordHashService>();
```

## 10. 确认不引入旧 InnerServer 逻辑

新版 meta-server 的用户体系只依赖本系统：

- 登录使用 Day 05 的本地 JWT。
- 用户通过 `/api/user/create` 自建。
- 搜索只查本地 `users` 表。
- 不创建 `IInnerServerUserClient`。
- 不创建 `Api/Integrations/InnerServer`。
- 不在 `Program.cs` 中注册 InnerServer `HttpClient`。

原 NestJS 的 `InnerServerService` 可以帮助你理解旧系统为什么要查权限中心，但今天不要迁移它。这样后续需求会更清楚：用户、密码、状态、角色都由当前 .NET 服务维护。

## 11. 创建 UserController

### Step 11.1 创建 `UserController.cs`

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

原 NestJS 里 `detail/:id` 方法声明了路由参数，但代码里读的是 query。今天直接使用 ASP.NET Core 的 route binding：`GET /api/user/detail/1` 会绑定到 `int id`。

## 12. 检查用户实体配置

### Step 12.1 打开 `User.cs`

确认：

```text
src/Domain/Entities/User.cs
```

至少有这些字段：

```csharp
public int Id { get; set; }
public string UserId { get; set; } = string.Empty;
public string? DingTalkUserId { get; set; }
public string? ManagerUserId { get; set; }
public string? ManagerDingTalkUserId { get; set; }
public string? Email { get; set; }
public string Name { get; set; } = string.Empty;
public string? RealName { get; set; }
public string Mobile { get; set; } = string.Empty;
public string PasswordHash { get; set; } = string.Empty;
public int Role { get; set; }
public int Status { get; set; }
public DateTimeOffset CreatedAt { get; set; }
public DateTimeOffset UpdatedAt { get; set; }
```

### Step 12.2 打开 `UserConfiguration.cs`

确认：

```text
src/Infrastructure/Persistence/Configurations/UserConfiguration.cs
```

有这些关键映射：

```csharp
builder.Property(entity => entity.DingTalkUserId)
    .HasColumnName("ding_talk_user_id")
    .HasMaxLength(64);

builder.Property(entity => entity.ManagerDingTalkUserId)
    .HasColumnName("manager_ding_talk_user_id")
    .HasMaxLength(64);

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

如果你看到类似：

```csharp
.HasColumnName(("password_hash"))
```

可以顺手改成：

```csharp
.HasColumnName("password_hash")
```

双括号能编译，但不必保留。

## 13. 编译并处理第一轮错误

### Step 13.1 执行 build

执行：

```bash
dotnet build
```

如果看到 `PagedResult<>` 找不到，检查：

- `src/Application/Common/PagedResult.cs` 是否存在。
- `UserController.cs` 是否有 `using Application.Common;`。
- `UserService.cs` 是否没有再引用 `Api.Responses`。

如果看到 FluentValidation 相关类型找不到，执行：

```bash
dotnet restore
```

再重新 build。

## 14. 手动调用接口

### Step 14.1 启动 API

执行：

```bash
dotnet run --project src/Api/Api.csproj
```

记下终端输出里的本地地址，例如：

```text
http://localhost:5063
```

如果端口不同，下面命令里的地址也一起替换。

### Step 14.2 登录拿 token

新开一个终端，执行：

```bash
curl -s -X POST http://localhost:5063/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"account":"13800000001","password":"123456"}'
```

从响应的 `data.accessToken` 复制 token。

### Step 14.3 查看用户分页

执行：

```bash
curl -s 'http://localhost:5063/api/user/list?pageIndex=1&pageSize=10' \
  -H 'Authorization: Bearer <accessToken>'
```

验收：

- HTTP 状态是 200。
- 响应外层仍然有 `success`、`code`、`message`、`data`、`requestId`。
- `data.items` 里能看到 Day 04 种子用户。

### Step 14.4 创建用户

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
- 数据库里保存的是 `PasswordHash`，不是明文 `password`。

再用新用户登录一次：

```bash
curl -s -X POST http://localhost:5063/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"account":"13900000001","password":"123456"}'
```

验收：

- 能拿到新的 `data.accessToken`。
- 说明自建用户已经接入 Day 05 的标准登录流程。

### Step 14.5 重复手机号

再次执行同一个创建命令。

验收：

- HTTP 状态是 400。
- `code` 是 `USER_MOBILE_EXISTS`。
- `message` 是 `手机号已存在`。

### Step 14.6 更新 manager

先确认已有种子用户 `u001`，然后执行：

```bash
curl -s -X PUT http://localhost:5063/api/user/update/2 \
  -H 'Content-Type: application/json' \
  -H 'Authorization: Bearer <accessToken>' \
  -d '{"managerUserId":"u001"}'
```

验收：

- 返回用户的 `managerUserId` 是 `u001`。
- 如果本地能找到 `u001`，可以确认 manager 关系已经建立。

### Step 14.7 搜索本地用户

执行：

```bash
curl -s 'http://localhost:5063/api/user/search?key=dev' \
  -H 'Authorization: Bearer <accessToken>'
```

验收：

- 如果本地 `name`、`realName` 或 `userId` 命中，返回本地用户。
- 如果本地没有命中，返回空数组，不调用外部系统。

## 15. 编写集成测试

测试放在最后，是因为今天先要把概念和功能跑通，再用测试固化行为。

### Step 15.1 创建 `UserApiTests.cs`

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

### Step 15.2 注意测试共享数据库状态

`TestEnvironmentFixture` 是 class fixture，同一个测试类里的测试共享一套临时数据库。上面的测试只依赖 Day 04 种子用户，所以分页数量可以断言为“至少有种子数据”，不要写得过度脆弱：

```csharp
Assert.True(data.GetProperty("totalCount").GetInt32() >= 2);
```

等你后面想练更严格的测试隔离时，可以升级为每个测试重建数据库，或每个测试用事务回滚。今天先不扩展。

## 16. 运行测试

### Step 16.1 执行 build

执行：

```bash
dotnet build
```

验收：

- 没有编译错误。
- 如果有 nullable warning，优先修掉，不要靠忽略通过。

### Step 16.2 执行全部测试

执行：

```bash
dotnet test
```

验收：

- Auth、Diagnostics、Database、Redis、User 相关测试全部通过。
- 测试输出中能看到 Testcontainers 启动 PostgreSQL 和 Redis。

### Step 16.3 如果搜索测试失败

优先检查三件事：

- `UserRepository.SearchLocalAsync` 是否同时查询了 `Name`、`RealName` 和 `UserId`。
- `SearchAsync` 是否对空关键词返回空数组。
- 本地无命中时是否直接返回空数组，没有额外创建用户。

### Step 16.4 如果重复手机号测试失败

检查：

- `CreateAsync` 里是否先 `Trim()` 了手机号。
- `ExistsByMobileOrUserIdAsync` 是否同时检查 `Mobile` 和 `UserId`。
- 种子数据里是否已经有 `13800000001`。

## 17. 今天的学习检查

完成后，回头确认你能解释这些问题：

- 为什么 Controller 不直接注入 `MetaServerDbContext`？
- 为什么 `Application` 层不能引用 `Api.Exceptions.BusinessException`？
- `AnyAsync` 和 `FirstOrDefaultAsync` 分别适合什么场景？
- 分页为什么要先 `CountAsync`，再 `Skip` / `Take`？
- 更新手机号时为什么要传 `excludingId`？
- 为什么新版用户搜索不再调用 InnerServer？
- FluentValidation 为什么今天用显式 `ValidateAsync`？

## 18. 今日验收清单

- [ ] `FluentValidation.DependencyInjectionExtensions` 已添加到 `Application`。
- [ ] `Application/Common` 中有业务异常、验证异常和分页结果。
- [ ] `Application/Users` 中有 DTO、验证器、常量、接口和 `UserService`。
- [ ] `Infrastructure/Users/UserRepository.cs` 用 EF Core LINQ 实现用户查询。
- [ ] `PersistenceRegistration` 注册了 `IUserRepository`。
- [ ] `Api/Controllers/UserController.cs` 提供了 Day 06 要求的 5 个接口。
- [ ] 全文没有要求创建 InnerServer、第三方用户查询或外部用户同步。
- [ ] `dotnet build` 通过。
- [ ] `dotnet test` 通过。

## 19. 晚上复盘

可以按这个格式记录到你的学习笔记：

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
