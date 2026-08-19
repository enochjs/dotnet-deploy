# ASP.NET Core 约定地图

> 目标：不是背 API，而是建立 ASP.NET Core 的“框架地图”。
>
> 阅读陌生代码时优先判断：
>
> 1. **谁创建的？**
> 2. **谁调用的？**
> 3. **数据从哪里来？**
> 4. **生命周期是什么？**

---

# 1. 总体地图

```text
ASP.NET Core
│
├── 1. DI / 依赖注入
│   ├── IServiceCollection
│   ├── AddSingleton
│   ├── AddScoped
│   ├── AddTransient
│   └── 构造函数自动注入
│
├── 2. Configuration / Options
│   ├── IConfiguration
│   ├── appsettings.json
│   ├── AddOptions<T>
│   ├── IOptions<T>
│   ├── IOptionsSnapshot<T>
│   ├── IOptionsMonitor<T>
│   └── IConfigureOptions<T>
│
├── 3. HTTP Pipeline / Middleware
│   ├── app.Use(...)
│   ├── UseMiddleware<T>()
│   ├── UseAuthentication()
│   ├── UseAuthorization()
│   └── MapControllers()
│
├── 4. MVC / Controller
│   ├── Controller
│   ├── Routing
│   ├── Model Binding
│   ├── Validation
│   ├── Filter
│   └── ApiBehaviorOptions
│
├── 5. Authentication / 身份认证
│   ├── Scheme
│   ├── AuthenticationHandler
│   ├── JwtBearerHandler
│   ├── Claim
│   ├── ClaimsIdentity
│   ├── ClaimsPrincipal
│   └── HttpContext.User
│
├── 6. Authorization / 权限授权
│   ├── [Authorize]
│   ├── Role
│   ├── Claim
│   ├── Policy
│   └── Requirement
│
├── 7. EF Core
│   ├── DbContext
│   ├── DbSet<T>
│   ├── Entity Configuration
│   ├── LINQ
│   ├── Include
│   ├── Change Tracking
│   └── SaveChanges
│
└── 8. Clean Architecture / 项目分层
    ├── Api
    ├── Application
    ├── Domain
    └── Infrastructure
```

---

# 2. ASP.NET Core 最重要的两个阶段

理解 ASP.NET Core 时，可以先把代码分成两个阶段。

## 2.1 注册阶段

典型代码：

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAuthentication()
    .AddJwtBearer();

builder.Services.AddAuthorization();

builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();
```

核心对象：

```text
IServiceCollection
```

可以理解为：

> 告诉 ASP.NET Core：“我的程序拥有哪些能力，以及这些对象应该怎么创建。”

---

## 2.2 请求阶段

```csharp
app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

这是：

```text
HTTP Request Pipeline
```

可以理解成：

> 一个 HTTP 请求进来以后，要依次经过哪些处理步骤。

所以最简单的记忆方式：

```text
builder.Services.AddXXX()
        ↓
注册能力


app.UseXXX()
        ↓
请求使用这个能力
```

例如：

```csharp
builder.Services.AddAuthentication();
app.UseAuthentication();
```

分别表示：

```text
AddAuthentication
→ 注册 Authentication 系统

UseAuthentication
→ HTTP 请求经过 Authentication Middleware
```

---

# 3. DI：依赖注入

ASP.NET Core 大量对象不是自己 `new` 的。

例如：

```csharp
public sealed class UserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }
}
```

你没有：

```csharp
new UserRepository()
```

因为 ASP.NET Core DI Container 会负责创建。

---

## 3.1 IServiceCollection

```csharp
builder.Services
```

类型：

```csharp
IServiceCollection
```

作用：

> 注册服务以及服务的创建规则。

例如：

```csharp
services.AddScoped<IUserRepository, UserRepository>();
```

意思：

```text
以后有人需要：

IUserRepository

        ↓

请创建：

UserRepository
```

---

# 4. 三种生命周期

最常见：

```csharp
AddSingleton
AddScoped
AddTransient
```

---

## 4.1 Singleton

```csharp
services.AddSingleton<XXX>();
```

整个应用生命周期只有一个实例。

```text
Application
│
├── Request A ──┐
├── Request B ──┼──→ XXX #1
├── Request C ──┤
└── Request D ──┘
```

适合：

```text
无状态服务
线程安全服务
全局配置器
缓存管理器
```

例如：

```csharp
services.AddSingleton<
    IConfigureOptions<JwtBearerOptions>,
    JwtBearerOptionsSetup
>();
```

---

## 4.2 Scoped

```csharp
services.AddScoped<XXX>();
```

一次 HTTP Request 一个实例。

```text
Request A
├── UserService #1
├── UserRepository #1
└── DbContext #1


Request B
├── UserService #2
├── UserRepository #2
└── DbContext #2
```

典型：

```text
DbContext
Repository
Application Service
CurrentUserAccessor
```

例如：

```csharp
services.AddScoped<
    ICurrentUserAccessor,
    CurrentUserAccessor
>();
```

---

## 4.3 Transient

```csharp
services.AddTransient<XXX>();
```

每次获取都创建一个新实例。

```text
Resolve XXX
    ↓
XXX #1

Resolve XXX
    ↓
XXX #2

Resolve XXX
    ↓
XXX #3
```

适合：

```text
非常轻量
无状态
创建成本很低
```

---

## 4.4 生命周期速查

| 生命周期 | 创建次数 | 常见用途 |
|---|---|---|
| Singleton | 整个应用一次 | 配置器、线程安全全局服务 |
| Scoped | 每个 Request 一次 | DbContext、Repository、业务 Service |
| Transient | 每次 Resolve | 轻量无状态 Service |

重要原则：

```text
生命周期短 → 依赖生命周期长
通常 OK

Scoped → Singleton
✅

Transient → Singleton
✅


生命周期长 → 依赖生命周期短
危险

Singleton → Scoped
❌
```

---

# 5. 构造函数注入

例如：

```csharp
public sealed class UserService(
    IUserRepository repository,
    ILogger<UserService> logger)
{
}
```

你没有写：

```csharp
new UserRepository();
new Logger();
```

框架会：

```text
创建 UserService
      ↓
检查构造函数
      ↓
需要 IUserRepository
      ↓
去 DI Container 找
      ↓
找到 UserRepository
      ↓
创建
      ↓
传给 UserService
```

所以看到构造函数参数时，要习惯问：

> 这个类型在哪里注册的？

---

# 6. Extension Method

ASP.NET Core 大量 API 都是扩展方法。

例如：

```csharp
services.AddControllers();
services.AddAuthentication();
services.AddAuthorization();
services.AddOptions<T>();
```

很多并不是：

```text
IServiceCollection
```

接口自己定义的方法。

而是：

```csharp
public static IServiceCollection AddXXX(
    this IServiceCollection services)
{
    ...
}
```

所以：

```csharp
services.AddXXX();
```

本质类似：

```csharp
SomeExtensions.AddXXX(services);
```

---

# 7. Configuration

ASP.NET Core 配置通常来自：

```text
appsettings.json
appsettings.Development.json
环境变量
命令行参数
Secret
其他 Configuration Provider
```

统一进入：

```csharp
IConfiguration
```

例如：

```json
{
  "Jwt": {
    "Issuer": "MetaServer",
    "Audience": "MetaServerApi"
  }
}
```

读取：

```csharp
configuration["Jwt:Issuer"];
```

---

# 8. Options Pattern

相比直接到处使用：

```csharp
IConfiguration
```

ASP.NET Core 更推荐：

```csharp
JwtOptions
```

例如：

```csharp
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;
}
```

然后：

```csharp
services
    .AddOptions<JwtOptions>()
    .BindConfiguration("Jwt");
```

关系：

```text
appsettings.json

"Jwt"
  ↓
Configuration
  ↓
BindConfiguration
  ↓
JwtOptions
```

---

# 9. IOptions<T>

常见：

```csharp
IOptions<JwtOptions>
```

使用：

```csharp
public JwtTokenService(
    IOptions<JwtOptions> options)
{
    var issuer = options.Value.Issuer;
}
```

注意：

```text
IOptions<T>
      ↓
.Value
      ↓
T
```

所以：

```csharp
options.Value
```

才是真正的：

```csharp
JwtOptions
```

---

# 10. Options Validation

例如：

```csharp
public sealed class JwtOptions
{
    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string SigningKey { get; init; } = string.Empty;
}
```

注册：

```csharp
services
    .AddOptions<JwtOptions>()
    .BindConfiguration("Jwt")
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

意思：

```text
读取配置
 ↓
绑定 JwtOptions
 ↓
执行 DataAnnotations 校验
 ↓
应用启动时立即校验
```

---

# 11. IConfigureOptions<T>

看到：

```csharp
IConfigureOptions<JwtBearerOptions>
```

可以理解为：

> 一个专门负责配置 `JwtBearerOptions` 的对象。

例如：

```csharp
public sealed class JwtBearerOptionsSetup
    : IConfigureOptions<JwtBearerOptions>
{
    public void Configure(
        JwtBearerOptions options)
    {
        options.TokenValidationParameters = ...;
    }
}
```

注册：

```csharp
services.AddSingleton<
    IConfigureOptions<JwtBearerOptions>,
    JwtBearerOptionsSetup
>();
```

之后 ASP.NET Core 创建 `JwtBearerOptions` 时会自动使用它。

---

# 12. Middleware

Middleware 是 ASP.NET Core HTTP Pipeline 的核心。

```text
Request
  ↓
Middleware A
  ↓
Middleware B
  ↓
Middleware C
  ↓
Controller
  ↓
Middleware C
  ↓
Middleware B
  ↓
Middleware A
  ↓
Response
```

类似洋葱。

---

# 13. 自定义 Middleware

例如：

```csharp
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        Console.WriteLine("Before");

        await _next(context);

        Console.WriteLine("After");
    }
}
```

注册：

```csharp
app.UseMiddleware<RequestLoggingMiddleware>();
```

框架负责创建 Middleware。

---

# 14. RequestDelegate

```csharp
RequestDelegate
```

可以简单理解：

> Pipeline 中的下一个处理函数。

所以：

```csharp
await _next(context);
```

就是：

```text
继续执行下一个 Middleware
```

如果不调用：

```csharp
await _next(context);
```

Pipeline 就在这里终止。

---

# 15. Middleware 顺序非常重要

例如：

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

不能随便交换。

因为：

```text
Authentication
      ↓
你是谁？

Authorization
      ↓
你有没有权限？
```

必须先知道：

```text
你是谁
```

才能判断：

```text
你能干什么
```

---

# 16. Authentication

Authentication：

> 身份认证——确认“你是谁”。

例如：

```text
JWT
Cookie
OAuth
OpenID Connect
Windows Authentication
```

ASP.NET Core 最终会统一转换成：

```csharp
ClaimsPrincipal
```

然后放到：

```csharp
HttpContext.User
```

---

# 17. Authentication Scheme

例如：

```csharp
services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme
);
```

这里：

```csharp
JwtBearerDefaults.AuthenticationScheme
```

实际上就是：

```text
Bearer
```

Scheme 可以理解：

> 使用哪套 Authentication 方案。

例如：

```text
Bearer
Cookies
Google
Microsoft
```

---

# 18. AddJwtBearer

```csharp
services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
```

意思：

```text
默认 Authentication Scheme
        ↓
Bearer

Bearer
        ↓
使用 JwtBearerHandler
```

---

# 19. JWT 请求完整流程

前端：

```http
GET /api/users/me

Authorization: Bearer eyJhbGci...
```

ASP.NET Core：

```text
HTTP Request
      ↓
UseAuthentication()
      ↓
Authentication System
      ↓
Scheme = Bearer
      ↓
JwtBearerHandler
      ↓
读取 Authorization Header
      ↓
提取 JWT
      ↓
验证 Signature
      ↓
验证 Issuer
      ↓
验证 Audience
      ↓
验证 Lifetime
      ↓
解析 Claims
      ↓
ClaimsIdentity
      ↓
ClaimsPrincipal
      ↓
HttpContext.User
```

---

# 20. Claim

Claim 可以简单理解：

> 关于当前用户的一条 key-value 身份信息。

例如：

```csharp
new Claim("userId", "U001")
new Claim("name", "fenghe")
new Claim("role", "Admin")
```

可以脑补：

```text
userId = U001
name   = fenghe
role   = Admin
```

---

# 21. ClaimsIdentity

多个 Claim 组成：

```csharp
ClaimsIdentity
```

例如：

```text
ClaimsIdentity
├── userId = U001
├── name = fenghe
├── mobile = 138...
└── role = Admin
```

---

# 22. ClaimsPrincipal

再往上一层：

```text
ClaimsPrincipal
└── ClaimsIdentity
    ├── Claim
    ├── Claim
    └── Claim
```

ASP.NET Core：

```csharp
HttpContext.User
```

类型就是：

```csharp
ClaimsPrincipal
```

---

# 23. HttpContext.User

这是 ASP.NET Core 的固定约定：

```csharp
HttpContext.User
```

表示：

> 当前 HTTP 请求认证出来的用户身份。

不是 JWT 独有。

无论：

```text
JWT
Cookie
OAuth
Windows Authentication
```

最终都可以统一：

```text
Authentication
      ↓
ClaimsPrincipal
      ↓
HttpContext.User
```

---

# 24. IHttpContextAccessor

Controller 可以直接：

```csharp
HttpContext.User
```

普通 Service 没有 `HttpContext` 属性。

所以可以：

```csharp
IHttpContextAccessor
```

例如：

```csharp
public sealed class CurrentUserAccessor
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserAccessor(
        IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }
}
```

然后：

```csharp
var user =
    _accessor.HttpContext?.User;
```

---

# 25. CurrentUserAccessor 的意义

不建议 Application 到处：

```csharp
HttpContext.User.FindFirstValue(...)
```

可以封装：

```text
HttpContext.User
      ↓
CurrentUserAccessor
      ↓
CurrentUser
```

Application：

```csharp
var user =
    currentUserAccessor.GetRequiredCurrentUser();

user.Id
user.Name
user.Role
```

这样 Application 不需要知道：

```text
HttpContext
ClaimsPrincipal
JWT
Claim
```

---

# 26. Authentication vs Authorization

一定要区分：

```text
Authentication
身份认证

“你是谁？”
```

和：

```text
Authorization
权限授权

“你能干什么？”
```

例如：

```text
JWT 验证成功

UserId = 123
Role = User

        ↓

Authentication 成功
```

然后访问：

```csharp
[Authorize(Roles = "Admin")]
```

发现：

```text
Role != Admin

        ↓

Authorization 失败
```

---

# 27. 401 vs 403

## 401 Unauthorized

实际语义：

> Authentication 失败。

例如：

```text
没有 Token
Token 无效
Token 过期
签名错误
```

结果：

```text
401
```

---

## 403 Forbidden

语义：

> 已经知道你是谁，但是你没有权限。

例如：

```text
UserId = 123
Role = User

Authentication
✅

要求 Admin

Authorization
❌

→ 403
```

记忆：

```text
401
你是谁我都不知道


403
我知道你是谁，但你不能进
```

---

# 28. [Authorize]

```csharp
[Authorize]
public IActionResult GetUsers()
{
}
```

`[Authorize]` 本身不是一个 Authentication Middleware。

它更接近：

> 给 Endpoint 添加 Authorization Metadata。

请求：

```text
Request
 ↓
UseAuthentication
 ↓
HttpContext.User
 ↓
UseAuthorization
 ↓
发现 Endpoint 有 [Authorize]
 ↓
检查用户
 ↓
Controller
```

---

# 29. Role Authorization

例如：

```csharp
[Authorize(Roles = "Admin")]
```

意思：

```text
当前用户必须：

IsAuthenticated = true

并且：

Role = Admin
```

---

# 30. Policy Authorization

复杂项目更推荐：

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy(
        "DeleteApplication",
        policy =>
        {
            policy.RequireClaim(
                "permission",
                "application:delete"
            );
        });
});
```

Controller：

```csharp
[Authorize(Policy = "DeleteApplication")]
```

JWT：

```text
permission = application:delete
```

于是：

```text
JWT
 ↓
Claims
 ↓
Authorization Policy
 ↓
permission 是否满足
```

---

# 31. MVC

ASP.NET Core Web API 中：

```csharp
services.AddControllers();
```

注册 MVC Controller 相关能力。

例如：

```csharp
[ApiController]
[Route("api/users")]
public sealed class UsersController
    : ControllerBase
{
}
```

---

# 32. Routing

例如：

```csharp
[Route("api/users")]
```

加：

```csharp
[HttpGet("{id}")]
```

最终：

```http
GET /api/users/123
```

ASP.NET Core Routing：

```text
HTTP Request
     ↓
匹配 Endpoint
     ↓
UsersController
     ↓
Get(id)
```

---

# 33. Model Binding

例如：

```csharp
[HttpGet("{id}")]
public IActionResult Get(int id)
```

请求：

```http
GET /users/123
```

框架自动：

```text
"123"
 ↓
int
 ↓
id = 123
```

你没有自己写：

```csharp
int.Parse(...)
```

这就是 Model Binding。

---

# 34. Body Model Binding

请求：

```json
{
  "name": "fenghe",
  "age": 18
}
```

Controller：

```csharp
public IActionResult Create(
    CreateUserRequest request)
```

ASP.NET Core 自动：

```text
JSON
 ↓
反序列化
 ↓
CreateUserRequest
```

---

# 35. Validation

例如：

```csharp
public sealed record CreateUserRequest(
    [Required]
    string Name
);
```

配合：

```csharp
[ApiController]
```

ASP.NET Core 会自动执行模型校验。

所以很多时候：

```csharp
if (request.Name == null)
```

不需要自己写。

---

# 36. ApiBehaviorOptions

主要控制 `[ApiController]` 的一些约定行为。

例如：

```csharp
.ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory =
        context =>
        {
            ...
        };
});
```

典型用途：

```text
Model Validation 失败
      ↓
Controller 甚至还没执行
      ↓
ApiBehaviorOptions
      ↓
自动生成 400 Response
```

---

# 37. MVC Filter

常见：

```text
Authorization Filter
Resource Filter
Action Filter
Exception Filter
Result Filter
```

例如：

```csharp
options.Filters.Add<ApiResponseFilter>();
```

Filter 比 Middleware 更靠近 MVC / Controller。

简单区分：

```text
Middleware
→ 整个 HTTP Pipeline

Filter
→ MVC / Controller Pipeline
```

---

# 38. Middleware vs Filter

```text
HTTP Request

    Middleware
        ↓
    Middleware
        ↓

    MVC
    ├── Filter
    ├── Controller
    └── Filter

        ↓
    Middleware

HTTP Response
```

适合 Middleware：

```text
全局异常
RequestId
日志
Authentication
CORS
```

适合 Filter：

```text
Action 前后处理
Result 包装
MVC 特有逻辑
```

---

# 39. EF Core

EF Core 是：

```text
ORM
```

核心关系：

```text
C# Entity
    ↕
EF Core
    ↕
Database Table
```

---

# 40. DbContext

```csharp
public sealed class MetaServerDbContext
    : DbContext
{
}
```

可以理解：

> 当前数据库会话 / 工作单元。

它负责：

```text
查询
Tracking
新增
修改
删除
SaveChanges
Transaction
```

---

# 41. DbSet<T>

例如：

```csharp
public DbSet<User> Users { get; set; }
```

大概对应：

```text
C#

DbSet<User>

        ↕

Database

users table
```

然后：

```csharp
dbContext.Users
```

就是查询 User 的入口之一。

---

# 42. LINQ + EF Core

例如：

```csharp
var user = await dbContext.Users
    .Where(x => x.Id == id)
    .FirstOrDefaultAsync();
```

这里不是简单地：

```text
把所有数据拉进内存再过滤
```

EF Core 会分析表达式：

```csharp
x => x.Id == id
```

转换成类似：

```sql
SELECT *
FROM users
WHERE id = @id
LIMIT 1;
```

---

# 43. Include

例如：

```csharp
var user = await dbContext.Users
    .Include(x => x.Roles)
    .FirstAsync(x => x.Id == id);
```

`Include` 表示：

> 查询 User 时，同时加载关联数据 Roles。

可以类比：

```text
User
  ↓
Include Roles
  ↓
User + Roles
```

---

# 44. Change Tracking

EF Core 默认会跟踪查询出来的 Entity。

例如：

```csharp
var user = await dbContext.Users
    .FirstAsync();

user.Name = "fenghe";

await dbContext.SaveChangesAsync();
```

你没有：

```csharp
dbContext.Users.Update(user);
```

很多情况下依然可以更新。

因为：

```text
查询 User
 ↓
EF 开始 Tracking
 ↓
修改 Name
 ↓
EF 检测变化
 ↓
SaveChanges
 ↓
UPDATE
```

---

# 45. AsNoTracking

如果只是查询：

```csharp
var users = await dbContext.Users
    .AsNoTracking()
    .ToListAsync();
```

表示：

> 不需要 EF 跟踪这些对象的变化。

适合纯查询场景。

---

# 46. SaveChanges

EF 中：

```csharp
dbContext.Users.Add(user);
```

通常只是：

```text
告诉 DbContext：

这个 Entity 是 Added 状态
```

真正执行 SQL：

```csharp
await dbContext.SaveChangesAsync();
```

大致：

```text
Add()
 ↓
Change Tracker
 ↓
Added

SaveChanges()
 ↓
生成 INSERT
 ↓
Database
```

---

# 47. Entity Configuration

例如：

```csharp
public sealed class UserConfiguration
    : IEntityTypeConfiguration<User>
{
    public void Configure(
        EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100);
    }
}
```

负责描述：

```text
Entity
    ↓
如何映射 Database
```

包括：

```text
Table
Column
Primary Key
Foreign Key
Index
Length
Relationship
Delete Behavior
```

---

# 48. Migration

EF Core Migration 可以理解：

```text
Entity Model 发生变化
        ↓
生成 Migration
        ↓
Migration 描述数据库结构变化
        ↓
应用到 Database
```

例如：

```text
User 增加 Mobile
        ↓
Migration
        ↓
ALTER TABLE users
ADD mobile ...
```

---

# 49. Clean Architecture 分层

你现在的项目：

```text
Api
Application
Domain
Infrastructure
```

可以先这么理解。

---

# 50. Api

负责：

```text
HTTP 世界
```

例如：

```text
Controller
Middleware
Filter
Authentication
Authorization
HTTP Request
HTTP Response
Swagger
```

典型：

```text
HTTP Request
     ↓
Api
```

---

# 51. Application

负责：

> “这个系统要完成什么业务操作？”

例如：

```text
Login
CreateApplication
DeleteApplication
DeployApplication
ApproveSupplier
```

可以理解成：

```text
Use Case / 业务流程编排
```

典型：

```csharp
LoginService
ApplicationService
CommandHandler
QueryHandler
```

---

# 52. Domain

负责：

> 核心业务概念和业务规则。

例如：

```text
User
Application
Pipeline
Supplier

ApplicationStatus
UserRole

领域规则
```

Domain 尽量不知道：

```text
HTTP
JWT
PostgreSQL
Redis
Serilog
ASP.NET Core
```

---

# 53. Infrastructure

负责：

> 技术实现细节。

例如：

```text
PostgreSQL
EF Core
Redis
JWT
OSS
Git
DingTalk
HTTP Client
```

例如：

```text
Application

IUserRepository
      ↑
      │
Infrastructure

UserRepository
```

Application：

```text
我要查询 User
```

Infrastructure：

```text
我使用 EF Core + PostgreSQL 帮你查询
```

---

# 54. 为什么 Interface 和 Implementation 分开

例如：

```text
Application

IUserCredentialRepository
```

和：

```text
Infrastructure

UserCredentialRepository
```

关系：

```text
Application
只定义：

“我需要一个查询 User 的能力”

          ↑ interface

Infrastructure
提供：

“我用 EF Core 实现”
```

这样 Application 不需要依赖 EF Core。

---

# 55. JWT 在分层中的位置

JWT 很适合帮助理解分层。

Application：

```csharp
public interface IJwtTokenService
{
    string CreateToken(User user);
}
```

Infrastructure / Api：

```csharp
public sealed class JwtTokenService
    : IJwtTokenService
{
    // JwtSecurityToken
    // SigningCredentials
    // SymmetricSecurityKey
}
```

Application 只知道：

```text
我要创建 Token
```

具体：

```text
JWT
HMAC
RSA
SigningKey
```

属于技术实现。

---

# 56. CurrentUserAccessor 的作用

Application 定义：

```csharp
public interface ICurrentUserAccessor
{
    CurrentUser GetRequiredCurrentUser();
}
```

Api 实现：

```text
HttpContext.User
       ↓
ClaimsPrincipal
       ↓
读取 Claims
       ↓
CurrentUser
```

这样 Application 不需要依赖：

```text
ASP.NET Core
HttpContext
JWT
ClaimsPrincipal
```

---

# 57. 一次完整 HTTP 请求到底发生什么

这是整张地图最重要的一部分。

假设：

```http
POST /api/applications

Authorization: Bearer xxx

{
    "name": "Test"
}
```

整个流程：

```text
HTTP Request
     ↓
Kestrel
     ↓
Middleware Pipeline
     ↓
RequestId Middleware
     ↓
Logging Middleware
     ↓
Exception Middleware
     ↓
Authentication Middleware
     │
     ├── 读取 Bearer Token
     ├── JwtBearerHandler
     ├── 验证 JWT
     ├── 创建 ClaimsPrincipal
     └── 设置 HttpContext.User
     ↓
Authorization Middleware
     │
     └── 检查 [Authorize]
     ↓
Routing / MVC
     ↓
Model Binding
     │
     └── JSON → CreateApplicationRequest
     ↓
Validation
     │
     └── DataAnnotations
     ↓
Action Filter
     ↓
Controller
     ↓
Application Service
     │
     ├── CurrentUserAccessor
     ├── Repository
     └── Domain
     ↓
Infrastructure
     │
     ├── EF Core
     ├── DbContext
     └── PostgreSQL
     ↓
SaveChanges
     ↓
Controller Result
     ↓
Result Filter
     ↓
JSON Response
     ↓
Logging Middleware
     ↓
HTTP Response
```

---

# 58. 和 NestJS 的大致对应

| NestJS | ASP.NET Core |
|---|---|
| Module Provider | IServiceCollection / DI |
| `@Injectable()` | DI 注册 |
| `@Inject()` | 构造函数类型自动解析 |
| Middleware | Middleware |
| Guard | Authorization |
| Passport Strategy | Authentication Handler |
| `JwtStrategy` | `JwtBearerHandler` |
| `request.user` | `HttpContext.User` |
| Pipe | Model Binding + Validation |
| Interceptor | Action / Result Filter |
| Exception Filter | Exception Filter / Middleware |
| Controller | Controller |
| Service | Application Service |
| TypeORM / Prisma | EF Core |
| Repository | Repository |
| ConfigModule | Configuration + Options |

注意：

> 只是帮助理解，不是严格的一一对应。

---

# 59. 看到 AddXXX 时怎么理解

以后看到：

```csharp
services.AddXXX();
```

先想：

> “它是不是在 DI / Framework 中注册某种能力？”

例如：

```csharp
AddControllers()
```

注册 MVC。

```csharp
AddAuthentication()
```

注册 Authentication。

```csharp
AddAuthorization()
```

注册 Authorization。

```csharp
AddDbContext()
```

注册 EF DbContext。

```csharp
AddOptions()
```

注册 Options。

---

# 60. 看到 UseXXX 时怎么理解

例如：

```csharp
app.UseXXX();
```

先想：

> “是不是在 HTTP Pipeline 中加入 Middleware？”

例如：

```csharp
UseAuthentication()
```

执行身份认证。

```csharp
UseAuthorization()
```

执行权限判断。

```csharp
UseCors()
```

处理 CORS。

```csharp
UseMiddleware<RequestLoggingMiddleware>()
```

加入自定义 Middleware。

---

# 61. 看到 Attribute 时怎么理解

例如：

```csharp
[Authorize]
[ApiController]
[HttpGet]
[Required]
```

不要第一反应认为：

> “这个 Attribute 自己在执行代码。”

很多 Attribute 更像：

```text
Metadata / 配置标记
```

然后由框架读取。

例如：

```text
[Authorize]
      ↓
Authorization 系统读取


[HttpGet]
      ↓
Routing 系统读取


[Required]
      ↓
Validation 系统读取
```

这是非常典型的：

```text
Convention + Metadata
```

---

# 62. 看到 Interface 时怎么查实现

例如：

```csharp
public UserService(
    IUserRepository repository)
```

不要只看：

```text
IUserRepository
```

继续找：

```csharp
services.AddScoped<
    IUserRepository,
    UserRepository
>();
```

这样才能建立：

```text
IUserRepository
      ↓
DI
      ↓
UserRepository
```

阅读 .NET 项目时，这一步非常重要。

---

# 63. 阅读陌生代码的四问法

以后看到陌生代码，固定问下面四个问题。

## ① 谁创建的？

例如：

```csharp
public UserService(
    IUserRepository repository)
```

问：

```text
UserService 谁 new？

IUserRepository 谁 new？
```

可能答案：

```text
DI Container
```

---

## ② 谁调用的？

例如：

```csharp
public Task InvokeAsync(
    HttpContext context)
```

你没有调用它。

那就问：

```text
谁调用 InvokeAsync？
```

答案：

```text
ASP.NET Core Middleware Pipeline
```

---

## ③ 数据从哪里来？

例如：

```csharp
HttpContext.User
```

问：

```text
User 从哪里来？
```

答案：

```text
Authentication Middleware
      ↓
JwtBearerHandler
      ↓
JWT Claims
```

---

## ④ 生命周期是什么？

例如：

```csharp
UserRepository
```

找：

```csharp
services.AddScoped<
    IUserRepository,
    UserRepository
>();
```

于是知道：

```text
生命周期 = HTTP Request
```

---

# 64. 再增加一个非常有用的问题：这是我的代码还是框架代码？

例如：

```csharp
services.AddAuthentication()
```

框架。

```csharp
services.AddMetaServerAuthentication()
```

项目自己封装。

```csharp
app.UseAuthentication()
```

框架。

```csharp
app.UseMiddleware<RequestLoggingMiddleware>()
```

框架机制 + 自己的 Middleware。

阅读时先分清：

```text
.NET / ASP.NET Core
第三方 Library
项目自己的 Infrastructure
项目自己的业务代码
```

理解速度会快很多。

---

# 65. 最终速查图

```text
                        ASP.NET Core

                            │
              ┌─────────────┴─────────────┐
              │                           │
          启动阶段                     请求阶段
              │                           │
     builder.Services                  Request
              │                           ↓
              │                      Middleware
              │                           ↓
        IServiceCollection          Authentication
              │                           ↓
     ┌────────┼─────────┐          HttpContext.User
     │        │         │                 ↓
     DI     Options   Framework       Authorization
     │        │                           ↓
     │        │                         MVC
     │        │                           ↓
Singleton IConfiguration            Model Binding
Scoped     IOptions<T>                    ↓
Transient  Validation                 Validation
                                         ↓
                                     Controller
                                         ↓
                                    Application
                                         ↓
                                       Domain
                                         ↓
                                  Infrastructure
                                         ↓
                                      EF Core
                                         ↓
                                     Database
```

---

# 66. 最值得优先掌握的顺序

如果不想一次记太多，推荐：

```text
第一阶段

DI
├── IServiceCollection
├── AddScoped
├── AddSingleton
└── 构造函数注入

        ↓

第二阶段

HTTP Pipeline
├── Middleware
├── app.UseXXX
└── HttpContext

        ↓

第三阶段

MVC
├── Controller
├── Routing
├── Model Binding
├── Validation
└── Filter

        ↓

第四阶段

Authentication
├── Scheme
├── Handler
├── Claims
└── HttpContext.User

        ↓

第五阶段

Authorization
├── [Authorize]
├── Role
└── Policy

        ↓

第六阶段

EF Core
├── DbContext
├── DbSet
├── LINQ
├── Include
├── Tracking
└── SaveChanges

        ↓

第七阶段

Clean Architecture
├── Api
├── Application
├── Domain
└── Infrastructure
```

---

# 67. 一句话心智模型

最后可以把整个 ASP.NET Core 压缩成一句话：

> **启动时通过 `builder.Services` 把各种能力和对象注册进 DI；请求进来后经过 `app.UseXXX` 组成的 Middleware Pipeline；Authentication 建立 `HttpContext.User`，Authorization 判断权限；MVC 完成路由、参数绑定和校验后调用 Controller；Controller 调 Application 执行业务流程，Application 使用 Domain 表达业务规则，通过 Infrastructure 操作数据库和外部系统。**

以后遇到陌生代码，不要先背 API。

先问：

```text
它属于哪套机制？

DI？
Options？
Middleware？
MVC？
Authentication？
Authorization？
EF Core？
Application？
Infrastructure？
```

一旦确定它在这张地图上的位置，再去研究具体 API，理解成本会低很多。