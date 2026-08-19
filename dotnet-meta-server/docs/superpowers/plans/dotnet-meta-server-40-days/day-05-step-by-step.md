# Day 05 Step-by-Step - 本地登录、JWT 鉴权和当前用户

这份文档是 `day-05.md` 的跟做版。今天会把后端从“能启动、能连库、能统一响应”，推进到“可以真实登录、签发 JWT、保护接口、读取当前用户”。

先明确今天的方向：这里不做临时演示式鉴权，不写“给一个任意 token 就放行”的临时代码，也不把密码明文放进数据库。我们按企业 .NET 项目的标准流程做：

- 用户密码只保存哈希。
- 登录接口用本地数据库里的 `users` 表校验账号和密码。
- JWT 由后端签发，包含必要 claims。
- API 使用 ASP.NET Core 标准 `JwtBearer` 认证。
- Controller 用 `[Authorize]` 控制访问。
- 业务代码通过 `ICurrentUserAccessor` 读取当前用户，而不是自己解析 Header。

原 NestJS 项目里 Day 05 对应的是 `AuthGuard + InnerServerService`：它从 Header 读取 Bearer token，再调用外部权限中心校验。当前 .NET 学习项目改成本地账号密码登录，是为了练习更完整、更标准的后端认证流程。InnerServer 查询用户的能力后续可以放到 Day 06 用户模块里。

原业务源码只作为功能参考；具体参考文件清单放在文档最后。

## 0. 今天最终要得到什么

完成后，你会在 Day 04 的项目上新增这些能力：

- `User` 实体新增 `PasswordHash` 字段。
- 数据库新增 migration：`AddUserPasswordHash`。
- 开发种子数据里有可以登录的用户。
- `JwtOptions` 配置可以绑定、校验和覆盖。
- 密码使用 `PasswordHasher<User>` 做哈希和验证。
- 登录接口 `POST /api/auth/login` 能签发 access token。
- 当前用户接口 `GET /api/auth/user` 需要登录后才能访问。
- JWT Bearer 鉴权接入 ASP.NET Core 标准认证管线。
- 401 响应仍然使用 Day 02 的统一响应结构。
- 集成测试覆盖登录成功、密码错误、无 token、非法 token、携带 token 访问当前用户。

你最后需要确认三件事：

- `dotnet build` 成功。
- `dotnet test` 成功。
- 用真实登录接口拿到 token 后，可以访问 `/api/auth/user`。

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

### Step 1.2 确认 Day 04 代码能编译

执行：

```bash
dotnet build
dotnet test
```

验收：

- `dotnet build` 能看到 `Build succeeded.`
- `dotnet test` 能看到 `Passed!`

如果这里失败，先修 Day 04。今天会改数据库结构、认证管线和集成测试，如果基础不干净，后面很难判断错误来自哪里。

## 2. 先理解今天的认证流程

### Step 2.1 把流程画在脑子里

今天的请求流程是：

```text
用户输入 account/password
        |
POST /api/auth/login
        |
AuthService 查询 users 表
        |
PasswordHashService 校验密码哈希
        |
JwtTokenService 签发 accessToken
        |
前端后续请求带 Authorization: Bearer <accessToken>
        |
JwtBearer handler 校验签名、issuer、audience、过期时间
        |
[Authorize] 放行
        |
CurrentUserAccessor 从 claims 读取当前用户
```

今天不要手写一个中间件去解析 token。ASP.NET Core 已经有成熟的认证管线，企业项目优先接入标准组件：

- `AddAuthentication().AddJwtBearer(...)` 负责识别和校验 token。
- `UseAuthentication()` 负责把 token 校验结果写入 `HttpContext.User`。
- `[Authorize]` 负责声明哪些接口需要登录。
- `ClaimsPrincipal` 是当前用户信息的标准载体。

## 3. 安装 JWT 和密码哈希依赖

### Step 3.1 给 Api 添加 JWT Bearer 包

执行：

```bash
dotnet add src/Api/Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.11
dotnet add src/Api/Api.csproj package System.IdentityModel.Tokens.Jwt --version 8.14.0
```

这两个包先这样理解：

- `Microsoft.AspNetCore.Authentication.JwtBearer`：把 JWT Bearer 接入 ASP.NET Core 认证管线。
- `System.IdentityModel.Tokens.Jwt`：创建和读取 JWT token。

### Step 3.2 给 Application 添加依赖注入抽象包

执行：

```bash
dotnet add src/Application/Application.csproj package Microsoft.Extensions.DependencyInjection.Abstractions --version 10.0.11
```

`Application` 层会提供 `AddMetaServerApplication()` 注册入口。它只需要依赖抽象包，不应该引用 ASP.NET Core 或 Infrastructure。

### Step 3.3 给 Infrastructure 添加密码哈希包

执行：

```bash
dotnet add src/Infrastructure/Infrastructure.csproj package Microsoft.Extensions.Identity.Core --version 10.0.11
```

`PasswordHasher<TUser>` 在这个包里。它会自动处理 salt 和哈希格式，不要自己写 SHA256、MD5 或固定 salt。

### Step 3.4 restore

执行：

```bash
dotnet restore
```

验收：

- restore 成功，没有红色错误。

## 4. 给用户实体增加密码哈希

### Step 4.1 修改 `User.cs`

打开：

```text
src/Domain/Entities/User.cs
```

增加 `PasswordHash`：

```csharp
namespace Domain.Entities;

public sealed class User
{
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
}
```

为什么字段叫 `PasswordHash`，不叫 `Password`：

- `Password` 容易让人误以为可以存明文。
- `PasswordHash` 明确表达数据库里保存的是哈希结果。
- 后续 code review 时，一眼就能发现有没有误用。

### Step 4.2 修改 `UserConfiguration.cs`

打开：

```text
src/Infrastructure/Persistence/Configurations/UserConfiguration.cs
```

在 `Mobile` 后面增加：

```csharp
builder.Property(entity => entity.PasswordHash)
    .HasColumnName("password_hash")
    .HasMaxLength(512)
    .IsRequired();
```

完整文件应类似：

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.UserId).HasColumnName("user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.DingTalkUserId).HasColumnName("ding_talk_user_id").HasMaxLength(64);
        builder.Property(entity => entity.ManagerUserId).HasColumnName("manager_user_id").HasMaxLength(64);
        builder.Property(entity => entity.ManagerDingTalkUserId).HasColumnName("manager_ding_talk_user_id").HasMaxLength(64);
        builder.Property(entity => entity.Email).HasColumnName("email").HasMaxLength(128);
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.RealName).HasColumnName("real_name").HasMaxLength(64);
        builder.Property(entity => entity.Mobile).HasColumnName("mobile").HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.PasswordHash).HasColumnName("password_hash").HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.Role).HasColumnName("role");
        builder.Property(entity => entity.Status).HasColumnName("status");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasIndex(entity => entity.Mobile)
            .HasDatabaseName("ix_users_mobile")
            .IsUnique();

        builder.HasIndex(entity => entity.UserId)
            .HasDatabaseName("ix_users_user_id")
            .IsUnique();

        builder.HasIndex(entity => entity.DingTalkUserId)
            .HasDatabaseName("ix_users_ding_talk_user_id");
    }
}
```

`512` 不是密码长度，是哈希字符串长度。ASP.NET Core Identity 的哈希结果会包含格式版本、salt、子密钥等信息，给足长度更稳。

今天登录支持 `userId` 或 `mobile`，所以建议顺手给 `user_id` 增加唯一索引。`mobile` 已经是唯一索引，`user_id` 也应该保持唯一，否则登录查询会出现多用户匹配风险。

## 5. 增加 JWT 配置

### Step 5.1 创建 `JwtOptions.cs`

创建文件：

```text
src/Api/Configuration/JwtOptions.cs
```

填入：

```csharp
using System.ComponentModel.DataAnnotations;

namespace Api.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    [MinLength(32)]
    public string SigningKey { get; init; } = string.Empty;

    [Range(5, 1440)]
    public int AccessTokenMinutes { get; init; } = 120;
}
```

今天先只做 access token，不做 refresh token。原因：

- Day 05 的核心是标准 JWT 登录和鉴权闭环。
- Refresh token 需要持久化、吊销、轮换、设备维度管理，适合单独一天做。
- 企业项目里不要把 refresh token 当成随手附赠的小字段。

### Step 5.2 注册 `JwtOptions`

打开：

```text
src/Api/Configuration/OptionsRegistration.cs
```

在 `AddApplicationOptions()` 中增加：

```csharp
services.AddValidatedOptions<JwtOptions>(JwtOptions.SectionName);
```

完整方法应类似：

```csharp
public static IServiceCollection AddApplicationOptions(this IServiceCollection services)
{
    services.AddValidatedOptions<PostgresOptions>(PostgresOptions.SectionName);
    services.AddValidatedOptions<RedisOptions>(RedisOptions.SectionName);
    services.AddValidatedOptions<GitOptions>(GitOptions.SectionName);
    services.AddValidatedOptions<DingTalkOptions>(DingTalkOptions.SectionName);
    services.AddValidatedOptions<OssOptions>(OssOptions.SectionName);
    services.AddValidatedOptions<InnerServerOptions>(InnerServerOptions.SectionName);
    services.AddValidatedOptions<MonitorOptions>(MonitorOptions.SectionName);
    services.AddValidatedOptions<LoggerOptions>(LoggerOptions.SectionName);
    services.AddValidatedOptions<JwtOptions>(JwtOptions.SectionName);
    return services;
}
```

### Step 5.3 修改开发配置

打开：

```text
src/Api/appsettings.Development.json
```

增加：

```json
"Jwt": {
  "Issuer": "dotnet-meta-server",
  "Audience": "dotnet-meta-web",
  "SigningKey": "dev-only-dotnet-meta-server-jwt-signing-key-please-change",
  "AccessTokenMinutes": 120
}
```

注意 JSON 逗号。最终结构大概是：

```json
{
  "Postgres": {
    "ConnectionString": "Host=localhost;Port=5432;Database=dotnet_meta_server;Username=root;Password=123456"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Jwt": {
    "Issuer": "dotnet-meta-server",
    "Audience": "dotnet-meta-web",
    "SigningKey": "dev-only-dotnet-meta-server-jwt-signing-key-please-change",
    "AccessTokenMinutes": 120
  }
}
```

上面只展示关键片段，不要删除文件里已有的 `Git`、`DingTalk`、`OSS`、`InnerServer`、`Monitor`、`Logger` 配置。

### Step 5.4 记住生产环境变量命名

生产环境不要把真实签名密钥提交到 git。按 ASP.NET Core 配置约定，环境变量可以这样写：

```bash
Jwt__Issuer=dotnet-meta-server
Jwt__Audience=dotnet-meta-web
Jwt__SigningKey=<至少32位的强随机字符串>
Jwt__AccessTokenMinutes=120
```

Linux/macOS 环境变量里用双下划线 `__` 表示配置层级。它会覆盖 `appsettings.json` 和 `appsettings.Development.json`。

## 6. 定义认证应用层接口和 DTO

### Step 6.1 创建目录

执行：

```bash
mkdir -p src/Application/Auth
```

### Step 6.1.1 先确认实体别名还在

Day 03 已经要求：引用领域实体 `Application` 时，写成

```csharp
using AppEntity = Domain.Entities.Application;
```

并从 `DbSet<AppEntity>`、`IEntityTypeConfiguration<AppEntity>`、`new AppEntity` 来使用它。

今天一开始写 `namespace Application.Auth`，就会把 `Application` 这个名字注册成命名空间。如果 Infrastructure 或测试里还在把 `Application` 当类型用，`dotnet build` 会报 CS0118。

打开并确认这些文件顶部有别名，且类型用法是 `AppEntity`：

```text
src/Infrastructure/Persistence/MetaServerDbContext.cs
src/Infrastructure/Persistence/Configurations/ApplicationConfiguration.cs
src/Infrastructure/Persistence/SeedData/DevelopmentSeedData.cs
tests/UnitTests/Persistence/MetaServerDbContextMetadataTests.cs
```

`Domain` 里的类名仍然是 `public sealed class Application`，不要改实体类名，也不要改表名 `applications`。

### Step 6.2 创建 `CurrentUser.cs`

创建文件：

```text
src/Application/Auth/CurrentUser.cs
```

填入：

```csharp
namespace Application.Auth;

public sealed record CurrentUser(
    int Id,
    string UserId,
    string Mobile,
    string Name,
    int Role);
```

`CurrentUser` 只放后端真正需要判断权限和归属的信息，不把密码哈希、邮箱等不必要信息塞进当前用户上下文。

### Step 6.3 创建 `LoginRequest.cs`

创建文件：

```text
src/Application/Auth/LoginRequest.cs
```

填入：

```csharp
using System.ComponentModel.DataAnnotations;

namespace Application.Auth;

public sealed class LoginRequest
{
    [Required]
    public string Account { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
```

`Account` 同时支持 `userId` 和 `mobile`。这样不需要新建用户名字段，也符合当前 `users` 表已有设计。

### Step 6.4 创建 `LoginResponse.cs`

创建文件：

```text
src/Application/Auth/LoginResponse.cs
```

填入：

```csharp
namespace Application.Auth;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    CurrentUser User);
```

### Step 6.5 创建 `AuthUserResponse.cs`

创建文件：

```text
src/Application/Auth/AuthUserResponse.cs
```

填入：

```csharp
namespace Application.Auth;

public sealed record AuthUserResponse(
    int Id,
    string UserId,
    string? Email,
    string Name,
    string? RealName,
    string Mobile,
    int Role,
    int Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
```

这个 DTO 对应原 NestJS 的 `UserDto`，但字段命名采用 .NET 风格。统一响应过滤器会把它包成：

```json
{
  "success": true,
  "code": "OK",
  "message": "success",
  "data": {
    "id": 1,
    "userId": "13800000001",
    "name": "meta-admin"
  },
  "requestId": "..."
}
```

### Step 6.6 创建认证相关接口

创建文件：

```text
src/Application/Auth/IUserCredentialRepository.cs
```

填入：

```csharp
using Domain.Entities;

namespace Application.Auth;

public interface IUserCredentialRepository
{
    Task<User?> FindByAccountAsync(string account, CancellationToken cancellationToken);
    Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken);
}
```

创建文件：

```text
src/Application/Auth/IPasswordHashService.cs
```

填入：

```csharp
using Domain.Entities;

namespace Application.Auth;

public interface IPasswordHashService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string password);
}
```

创建文件：

```text
src/Application/Auth/IJwtTokenService.cs
```

填入：

```csharp
namespace Application.Auth;

public interface IJwtTokenService
{
    LoginResponse CreateAccessToken(CurrentUser user);
}
```

创建文件：

```text
src/Application/Auth/ICurrentUserAccessor.cs
```

填入：

```csharp
namespace Application.Auth;

public interface ICurrentUserAccessor
{
    CurrentUser GetRequiredCurrentUser();
}
```

为什么要先定义接口：

- `Application` 层表达业务需要什么能力。
- `Infrastructure` 层负责用数据库和密码库实现这些能力。
- `Api` 层负责把 HTTP、JWT、Controller 接起来。
- 后续单元测试可以替换这些接口，不需要启动完整 Web 服务器。

## 7. 实现登录业务服务

### Step 7.1 创建 `AuthService.cs`

创建文件：

```text
src/Application/Auth/AuthService.cs
```

填入：

```csharp
using Domain.Entities;

namespace Application.Auth;

public sealed class AuthService
{
    private readonly IUserCredentialRepository _users;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUserCredentialRepository users,
        IPasswordHashService passwordHashService,
        IJwtTokenService jwtTokenService)
    {
        _users = users;
        _passwordHashService = passwordHashService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var account = request.Account.Trim();
        var user = await _users.FindByAccountAsync(account, cancellationToken);

        if (user is null || !_passwordHashService.VerifyPassword(user, request.Password))
        {
            throw new InvalidCredentialsException();
        }

        return _jwtTokenService.CreateAccessToken(ToCurrentUser(user));
    }

    public async Task<AuthUserResponse> GetCurrentUserAsync(CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdAsync(currentUser.Id, cancellationToken);

        if (user is null)
        {
            throw new CurrentUserNotFoundException();
        }

        return ToResponse(user);
    }

    private static CurrentUser ToCurrentUser(User user)
    {
        return new CurrentUser(
            user.Id,
            user.UserId,
            user.Mobile,
            user.Name,
            user.Role);
    }

    private static AuthUserResponse ToResponse(User user)
    {
        return new AuthUserResponse(
            user.Id,
            user.UserId,
            user.Email,
            user.Name,
            user.RealName,
            user.Mobile,
            user.Role,
            user.Status,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
```

这里没有返回 `null` 表示登录失败，而是抛业务异常。原因：

- Controller 不需要知道登录失败细节。
- 密码错误和账号不存在统一提示，避免枚举账号。
- Day 02 的异常中间件会把业务异常变成稳定 JSON。

### Step 7.2 创建认证业务异常

创建文件：

```text
src/Application/Auth/InvalidCredentialsException.cs
```

填入：

```csharp
namespace Application.Auth;

public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("账号或密码错误")
    {
    }
}
```

创建文件：

```text
src/Application/Auth/CurrentUserNotFoundException.cs
```

填入：

```csharp
namespace Application.Auth;

public sealed class CurrentUserNotFoundException : Exception
{
    public CurrentUserNotFoundException()
        : base("当前用户不存在")
    {
    }
}
```

### Step 7.3 注册应用层服务

创建文件：

```text
src/Application/ApplicationRegistration.cs
```

填入：

```csharp
using Application.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationRegistration
{
    public static IServiceCollection AddMetaServerApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        return services;
    }
}
```

这样 `Api` 只需要调用一个入口，不需要知道 Application 层内部有多少服务。

## 8. 实现数据库用户查询和密码服务

### Step 8.1 创建目录

执行：

```bash
mkdir -p src/Infrastructure/Auth
```

### Step 8.2 创建 `UserCredentialRepository.cs`

创建文件：

```text
src/Infrastructure/Auth/UserCredentialRepository.cs
```

填入：

```csharp
using Application.Auth;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Auth;

public sealed class UserCredentialRepository : IUserCredentialRepository
{
    private readonly MetaServerDbContext _dbContext;

    public UserCredentialRepository(MetaServerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> FindByAccountAsync(string account, CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.UserId == account || user.Mobile == account,
                cancellationToken);
    }

    public Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }
}
```

登录查询用 `AsNoTracking()`，因为这里只读用户信息，不需要 EF 跟踪变化。真实项目里，读多写少的查询都应该主动考虑是否需要 tracking。

### Step 8.3 创建 `PasswordHashService.cs`

创建文件：

```text
src/Infrastructure/Auth/PasswordHashService.cs
```

填入：

```csharp
using Application.Auth;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Auth;

public sealed class PasswordHashService : IPasswordHashService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string password)
    {
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return false;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        return result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
```

`SuccessRehashNeeded` 的意思是旧哈希还能用，但算法参数建议升级。今天先把它当作登录成功；后续做用户模块时，可以登录后顺手更新哈希。

### Step 8.4 注册 Infrastructure 服务

打开：

```text
src/Infrastructure/Persistence/PersistenceRegistration.cs
```

增加 using：

```csharp
using Application.Auth;
using Infrastructure.Auth;
```

在 `AddMetaServerPersistence()` 里注册：

```csharp
services.AddScoped<IUserCredentialRepository, UserCredentialRepository>();
services.AddSingleton<IPasswordHashService, PasswordHashService>();
```

完整文件应类似：

```csharp
using Application.Auth;
using Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

public static class PersistenceRegistration
{
    public static IServiceCollection AddMetaServerPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("Postgres")["ConnectionString"];

        services.AddDbContext<MetaServerDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUserCredentialRepository, UserCredentialRepository>();
        services.AddSingleton<IPasswordHashService, PasswordHashService>();

        return services;
    }
}
```

## 9. 实现 JWT 签发和当前用户访问器

### Step 9.1 创建目录

执行：

```bash
mkdir -p src/Api/Auth
```

### Step 9.2 创建 `JwtClaimTypes.cs`

创建文件：

```text
src/Api/Auth/JwtClaimTypes.cs
```

填入：

```csharp
namespace Api.Auth;

public static class JwtClaimTypes
{
    public const string UserId = "user_id";
    public const string Mobile = "mobile";
    public const string Name = "name";
    public const string Role = "role";
}
```

这些是项目自己的 claim 名称。`sub` 仍然用于用户数据库主键，其他业务字段用清晰的自定义名称。

### Step 9.3 创建 `JwtTokenService.cs`

创建文件：

```text
src/Api/Auth/JwtTokenService.cs
```

填入：

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Configuration;
using Application.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public LoginResponse CreateAccessToken(CurrentUser user)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(JwtClaimTypes.UserId, user.UserId),
            new Claim(JwtClaimTypes.Mobile, user.Mobile),
            new Claim(JwtClaimTypes.Name, user.Name),
            new Claim(JwtClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new LoginResponse(accessToken, expiresAt, user);
    }
}
```

这里把 `Id` 放进 `sub`，因为 `sub` 是 JWT 标准里的 subject。业务里的 `UserId` 另放到 `user_id`，避免两个概念混在一起。

`MapInboundClaims = false` 很重要。不开这个设置时，部分 .NET JWT 组件会把 `sub` 映射成较长的框架 claim type，导致你在 `CurrentUserAccessor` 里用 `JwtRegisteredClaimNames.Sub` 读不到用户 id。

### Step 9.4 创建 `CurrentUserAccessor.cs`

创建文件：

```text
src/Api/Auth/CurrentUserAccessor.cs
```

填入：

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Auth;

namespace Api.Auth;

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUser GetRequiredCurrentUser()
    {
        var principal = _httpContextAccessor.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("当前请求未登录");
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
```

业务 Service 不应该自己从 Header 里切字符串。它只知道“我要当前用户”，具体当前用户怎么从 HTTP 请求里来，是 Api 层的责任。

## 10. 注册标准 JWT Bearer 鉴权

### Step 10.1 创建 `JwtBearerOptionsSetup.cs`

企业项目里不要在 `Program.cs` 或 `AddJwtBearer(...)` 里临时 `BuildServiceProvider()` 取配置。标准做法是把复杂 options 配置放到独立 setup class，让依赖注入容器正常提供 `IOptions<JwtOptions>`。

创建文件：

```text
src/Api/Auth/JwtBearerOptionsSetup.cs
```

填入：

```csharp
using System.Text;
using Api.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Auth;

public sealed class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtOptions;

    public JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme)
        {
            return;
        }

        Configure(options);
    }

    public void Configure(JwtBearerOptions options)
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = false;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    }
}
```

### Step 10.2 创建 `AuthenticationRegistration.cs`

创建文件：

```text
src/Api/Auth/AuthenticationRegistration.cs
```

填入：

```csharp
using Application.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Api.Auth;

public static class AuthenticationRegistration
{
    public static IServiceCollection AddMetaServerAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

        return services;
    }
}
```

学习时要记住：配置复杂起来后，用 options setup class，比在 `Program.cs` 里写一大坨配置更好维护，也能避免临时构建第二个 service provider。

## 11. 修改异常处理中间件

### Step 11.1 处理未登录访问器抛出的异常

打开：

```text
src/Api/Middleware/ExceptionHandlingMiddleware.cs
```

在 `BusinessException` catch 后面增加：

```csharp
catch (UnauthorizedAccessException)
{
    await WriteErrorAsync(
        context,
        HttpStatusCode.Unauthorized,
        "UNAUTHORIZED",
        "请先登录"
    );
}
```

再增加对 Application 认证异常的处理：

```csharp
catch (InvalidCredentialsException exception)
{
    await WriteErrorAsync(
        context,
        HttpStatusCode.BadRequest,
        "INVALID_CREDENTIALS",
        exception.Message
    );
}
catch (CurrentUserNotFoundException exception)
{
    await WriteErrorAsync(
        context,
        HttpStatusCode.Unauthorized,
        "CURRENT_USER_NOT_FOUND",
        exception.Message
    );
}
```

文件顶部增加：

```csharp
using Application.Auth;
```

企业项目里，登录失败通常返回 400 或 401 都可以，但要团队统一。这里使用 400 + `INVALID_CREDENTIALS`，表示请求里的账号密码不成立；缺 token、token 错误、当前用户缺失使用 401。

## 12. 实现 AuthController

### Step 12.1 创建目录

执行：

```bash
mkdir -p src/Api/Controllers
```

如果目录已经存在，可以继续。

### Step 12.2 创建 `AuthController.cs`

创建文件：

```text
src/Api/Controllers/AuthController.cs
```

填入：

```csharp
using Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public AuthController(AuthService authService, ICurrentUserAccessor currentUserAccessor)
    {
        _authService = authService;
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public Task<LoginResponse> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        return _authService.LoginAsync(request, cancellationToken);
    }

    [HttpGet("user")]
    [Authorize]
    public Task<AuthUserResponse> GetCurrentUser(CancellationToken cancellationToken)
    {
        var currentUser = _currentUserAccessor.GetRequiredCurrentUser();
        return _authService.GetCurrentUserAsync(currentUser, cancellationToken);
    }
}
```

Controller 只做三件事：

- 声明路由。
- 声明权限。
- 调用应用服务。

不要把密码校验、JWT 生成、数据库查询写进 Controller。Controller 一胖，后面每个业务接口都会变得难测。

## 13. 修改 Program.cs

### Step 13.1 引入注册入口

打开：

```text
src/Api/Program.cs
```

顶部增加：

```csharp
using Api.Auth;
using Application;
```

### Step 13.2 注册 Application 和认证服务

找到：

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();
builder.Services.AddApplicationOptions();
builder.Services.AddMetaServerPersistence(builder.Configuration);
```

改成：

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationOptions();
builder.Services.AddMetaServerApplication();
builder.Services.AddMetaServerPersistence(builder.Configuration);
builder.Services.AddMetaServerAuthentication();
builder.Services.AddAuthorization();
```

顺序上，先注册 options，再注册依赖 options 的认证服务，会更容易理解。

### Step 13.3 加上 `UseAuthentication()`

找到：

```csharp
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
```

改成：

```csharp
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

`UseAuthentication()` 必须在 `UseAuthorization()` 前面。认证先识别“你是谁”，授权再判断“你能不能访问”。

### Step 13.4 用脚本同步种子数据

本地登录用的是 `DevelopmentSeedData`，不是 `dotnet ef database update`。update 只建表。刚 `database drop` 过、或 `password_hash` 仍是空时，在项目根目录执行：

```bash
./scripts/seed-dev.sh
```

脚本会先应用 pending migration，再调用 `SeedAsync()`。已有用户时会跳过，所以空哈希的旧用户不会被自动修好；那种情况要先 drop 再跑脚本。

## 14. 更新种子数据密码

### Step 14.1 打开开发种子数据

打开：

```text
src/Infrastructure/Persistence/SeedData/DevelopmentSeedData.cs
```

顶部增加：

```csharp
using Microsoft.AspNetCore.Identity;
```

在创建用户的地方，为每个种子用户设置密码哈希。建议今天统一使用学习密码：

```text
Meta@123456
```

代码结构可以这样写：

```csharp
var passwordHasher = new PasswordHasher<User>();
var admin = new User
{
    UserId = "13800000001",
    Email = "admin@example.com",
    Name = "meta-admin",
    RealName = "Meta Admin",
    Mobile = "13800000001",
    Role = 1,
    Status = 1,
    CreatedAt = now,
    UpdatedAt = now
};

admin.PasswordHash = passwordHasher.HashPassword(admin, "Meta@123456");
```

如果文件里已经有 `new User { ... }`，不要为了复制这段而删掉已有字段。只需要保证每个用户保存前都有：

```csharp
user.PasswordHash = passwordHasher.HashPassword(user, "Meta@123456");
```

### Step 14.2 保持 seed 幂等

如果 `DevelopmentSeedData` 已经有“存在则跳过”的逻辑，不要每次启动都重写密码哈希。

推荐写法：

```csharp
if (!await dbContext.Users.AnyAsync(cancellationToken))
{
    var passwordHasher = new PasswordHasher<User>();
    var users = new List<User>
    {
        new()
        {
            UserId = "13800000001",
            Email = "admin@example.com",
            Name = "meta-admin",
            RealName = "Meta Admin",
            Mobile = "13800000001",
            Role = 1,
            Status = 1,
            CreatedAt = now,
            UpdatedAt = now
        },
        new()
        {
            UserId = "13800000002",
            Email = "developer@example.com",
            Name = "meta-dev",
            RealName = "Meta Developer",
            Mobile = "13800000002",
            Role = 2,
            Status = 1,
            CreatedAt = now,
            UpdatedAt = now
        }
    };

    foreach (var user in users)
    {
        user.PasswordHash = passwordHasher.HashPassword(user, "Meta@123456");
    }

    dbContext.Users.AddRange(users);
    await dbContext.SaveChangesAsync(cancellationToken);
}
```

如果现有 seed 里用户和其他对象在同一个大方法里，也没关系。核心原则是：不要每跑一次 seed 就把已有用户密码重新随机改掉。

## 15. 生成数据库 migration

### Step 15.1 确认 EF CLI 可用

执行：

```bash
dotnet tool restore
dotnet ef dbcontext list \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```

验收：

```text
Infrastructure.Persistence.MetaServerDbContext
```

### Step 15.2 生成 migration

执行：

```bash
dotnet ef migrations add AddUserPasswordHash \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj \
  --output-dir Persistence/Migrations
```

验收：

```bash
find src/Infrastructure/Persistence/Migrations -type f | sort
```

你应该能看到类似：

```text
src/Infrastructure/Persistence/Migrations/20260818******_AddUserPasswordHash.cs
src/Infrastructure/Persistence/Migrations/20260818******_AddUserPasswordHash.Designer.cs
```

### Step 15.3 人工检查 migration

打开新生成的 `*_AddUserPasswordHash.cs`，核心内容应该类似：

```csharp
migrationBuilder.AddColumn<string>(
    name: "password_hash",
    table: "users",
    type: "character varying(512)",
    maxLength: 512,
    nullable: false,
    defaultValue: "");
```

如果你新增了 `user_id` 唯一索引，还应该看到类似：

```csharp
migrationBuilder.CreateIndex(
    name: "ix_users_user_id",
    table: "users",
    column: "user_id",
    unique: true);
```

学习项目里现有开发库可能已有用户，所以 EF 会给非空列加 `defaultValue: ""`。这能让 migration 应用成功，但不要把空密码哈希当成合法业务状态。后续 seed 或脚本要补齐真实哈希。

如果生产已有用户，真实企业流程应该是：

1. 先加可空列。
2. 后台批量初始化或走重置密码流程。
3. 再把列改成非空。

今天是新学习项目，可以接受一次性非空列。

### Step 15.4 更新本地数据库

执行：

```bash
dotnet ef database update \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```

验收：

- 命令最后看到 `Done.`
- 如果本机数据库没启动，可以先跳过本地 update，后面集成测试会用 Testcontainers 验证 migration。

## 16. 更新配置绑定测试

### Step 16.1 修改 `OptionsBindingTests.cs`

打开：

```text
tests/UnitTests/OptionsBindingTests.cs
```

在配置字典里增加：

```csharp
["Jwt:Issuer"] = "dotnet-meta-server",
["Jwt:Audience"] = "dotnet-meta-web",
["Jwt:SigningKey"] = "unit-test-dotnet-meta-server-jwt-signing-key",
["Jwt:AccessTokenMinutes"] = "120",
```

再增加一个测试：

```csharp
[Fact]
public void JwtOptions_BindsFromConfiguration()
{
    var services = new ServiceCollection();
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "dotnet-meta-server",
            ["Jwt:Audience"] = "dotnet-meta-web",
            ["Jwt:SigningKey"] = "unit-test-dotnet-meta-server-jwt-signing-key",
            ["Jwt:AccessTokenMinutes"] = "60"
        })
        .Build();

    services.AddSingleton<IConfiguration>(configuration);
    services.AddOptions<JwtOptions>()
        .BindConfiguration(JwtOptions.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();

    using var provider = services.BuildServiceProvider(validateScopes: true);
    var options = provider.GetRequiredService<IOptions<JwtOptions>>().Value;

    Assert.Equal("dotnet-meta-server", options.Issuer);
    Assert.Equal("dotnet-meta-web", options.Audience);
    Assert.Equal(60, options.AccessTokenMinutes);
}
```

为什么不直接调用 `AddApplicationOptions()`：

- `AddApplicationOptions()` 会校验所有配置。
- 这个测试只关心 `JwtOptions`。
- 单一目标的测试失败时更容易定位。

## 17. 增加认证集成测试

### Step 17.1 修改测试环境配置

打开：

```text
tests/IntegrationTests/Support/TestEnvironmentFixture.cs
```

在 `overrides` 里增加：

```csharp
["Jwt:Issuer"] = "dotnet-meta-server-tests",
["Jwt:Audience"] = "dotnet-meta-web-tests",
["Jwt:SigningKey"] = "integration-test-dotnet-meta-server-jwt-signing-key",
["Jwt:AccessTokenMinutes"] = "120",
```

这样集成测试不会依赖你本机 `appsettings.Development.json` 的 JWT 配置。

### Step 17.2 创建 `AuthApiTests.cs`

创建文件：

```text
tests/IntegrationTests/AuthApiTests.cs
```

填入：

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Api.Auth;
using IntegrationTests.Support;
using Microsoft.IdentityModel.Tokens;

namespace IntegrationTests;

public sealed class AuthApiTests : IClassFixture<TestEnvironmentFixture>
{
    private const string JwtIssuer = "dotnet-meta-server-tests";
    private const string JwtAudience = "dotnet-meta-web-tests";
    private const string JwtSigningKey = "integration-test-dotnet-meta-server-jwt-signing-key";

    private readonly TestEnvironmentFixture _fixture;

    public AuthApiTests(TestEnvironmentFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Login_WithValidPassword_ReturnsAccessToken()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            account = "13800000001",
            password = "Meta@123456"
        });
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<LoginResult>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.False(string.IsNullOrWhiteSpace(body.Data.AccessToken));
        Assert.Equal("13800000001", body.Data.User.UserId);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsStableBusinessError()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            account = "13800000001",
            password = "wrong-password"
        });
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal("INVALID_CREDENTIALS", body.Code);
    }

    [Fact]
    public async Task CurrentUser_WithoutToken_ReturnsUnauthorized()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/api/auth/user");
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal("UNAUTHORIZED", body.Code);
    }

    [Fact]
    public async Task CurrentUser_WithInvalidToken_ReturnsUnauthorized()
    {
        var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        var response = await client.GetAsync("/api/auth/user");
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal("UNAUTHORIZED", body.Code);
    }

    [Fact]
    public async Task CurrentUser_WithExpiredToken_ReturnsUnauthorized()
    {
        var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateExpiredToken());

        var response = await client.GetAsync("/api/auth/user");
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal("UNAUTHORIZED", body.Code);
    }

    [Fact]
    public async Task CurrentUser_WithValidToken_ReturnsDatabaseUser()
    {
        var client = _fixture.Factory.CreateClient();
        var login = await LoginAsync(client);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var response = await client.GetAsync("/api/auth/user");
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<AuthUserResult>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.Equal("13800000001", body.Data.UserId);
        Assert.Equal("meta-admin", body.Data.Name);
    }

    private static async Task<LoginResult> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            account = "13800000001",
            password = "Meta@123456"
        });
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<LoginResult>>();

        Assert.NotNull(body?.Data);
        return body.Data;
    }

    private static string CreateExpiredToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "1"),
                new Claim(JwtClaimTypes.UserId, "13800000001"),
                new Claim(JwtClaimTypes.Mobile, "13800000001"),
                new Claim(JwtClaimTypes.Name, "meta-admin"),
                new Claim(JwtClaimTypes.Role, "1")
            },
            notBefore: DateTime.UtcNow.AddMinutes(-20),
            expires: DateTime.UtcNow.AddMinutes(-10),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record ApiEnvelope<T>(
        bool Success,
        string Code,
        string Message,
        T? Data,
        string RequestId);

    private sealed record LoginResult(
        string AccessToken,
        DateTimeOffset ExpiresAt,
        CurrentUserResult User);

    private sealed record CurrentUserResult(
        int Id,
        string UserId,
        string Mobile,
        string Name,
        int Role);

    private sealed record AuthUserResult(
        int Id,
        string UserId,
        string? Email,
        string Name,
        string? RealName,
        string Mobile,
        int Role,
        int Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);
}
```

这些测试是端到端验证：

- 真实 API 管线。
- 真实 PostgreSQL 容器。
- 真实 migration。
- 真实 seed 用户。
- 真实密码哈希验证。
- 真实 JWT 签发和校验。

这比 mock 一个 `IAuthService` 更慢，但更能证明 Day 05 的登录闭环真的可用。

## 18. 测试当前用户访问器

### Step 18.1 创建轻量单元测试

集成测试已经覆盖了真实请求管线，但 `CurrentUserAccessor` 的错误分支适合用单元测试补一下。

创建文件：

```text
tests/UnitTests/Auth/CurrentUserAccessorTests.cs
```

如果目录不存在，先执行：

```bash
mkdir -p tests/UnitTests/Auth
```

填入：

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.Auth;
using Microsoft.AspNetCore.Http;

namespace UnitTests.Auth;

public sealed class CurrentUserAccessorTests
{
    [Fact]
    public void GetRequiredCurrentUser_WithClaims_ReturnsCurrentUser()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "1"),
                new Claim(JwtClaimTypes.UserId, "13800000001"),
                new Claim(JwtClaimTypes.Mobile, "13800000001"),
                new Claim(JwtClaimTypes.Name, "meta-admin"),
                new Claim(JwtClaimTypes.Role, "1")
            }, authenticationType: "Bearer"))
        };
        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var currentUserAccessor = new CurrentUserAccessor(accessor);

        var currentUser = currentUserAccessor.GetRequiredCurrentUser();

        Assert.Equal(1, currentUser.Id);
        Assert.Equal("13800000001", currentUser.UserId);
        Assert.Equal("meta-admin", currentUser.Name);
    }

    [Fact]
    public void GetRequiredCurrentUser_WithoutAuthenticatedIdentity_Throws()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var currentUserAccessor = new CurrentUserAccessor(accessor);

        Assert.Throws<UnauthorizedAccessException>(() =>
            currentUserAccessor.GetRequiredCurrentUser());
    }
}
```

单元测试不需要启动 WebApplicationFactory，也不需要数据库，跑起来很快。企业项目里，集成测试验证主路径，单元测试补充边界分支，是比较健康的组合。

## 19. 调整已有 401 测试

### Step 19.1 检查 Diagnostics 的安全接口

Day 02 里已有：

```text
tests/IntegrationTests/DiagnosticsApiTests.cs
```

里面有一个：

```csharp
SecureEndpoint_ReturnsUnifiedUnauthorizedError
```

今天接入 JWT 后，这个测试仍应该通过。它可以证明：

- `[Authorize]` 没 token 时会挑战认证。
- `AuthorizationResultHandler` 仍能输出统一 401。

如果测试失败并返回空 body，检查：

- `Program.cs` 是否注册了 `IAuthorizationMiddlewareResultHandler`。
- `app.UseAuthentication()` 是否在 `app.UseAuthorization()` 前。
- `AuthorizationResultHandler` 是否仍在写 `ApiResponse<object>.Fail(...)`。

## 20. 运行测试

### Step 20.1 先编译

执行：

```bash
dotnet build
```

验收：

```text
Build succeeded.
```

常见编译错误：

```text
The type or namespace name 'JwtBearer' does not exist
```

处理：

```bash
dotnet add src/Api/Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.11
dotnet restore
```

另一个常见错误：

```text
The type or namespace name 'PasswordHasher<>' could not be found
```

处理：

```bash
dotnet add src/Infrastructure/Infrastructure.csproj package Microsoft.Extensions.Identity.Core --version 10.0.11
dotnet restore
```

### Step 20.2 跑单元测试

执行：

```bash
dotnet test tests/UnitTests/UnitTests.csproj
```

验收：

```text
Passed!
```

### Step 20.3 跑集成测试

执行：

```bash
dotnet test tests/IntegrationTests/IntegrationTests.csproj
```

验收：

```text
Passed!
```

如果 Docker 没启动，会看到类似：

```text
Docker is either not running or misconfigured
```

打开 Docker Desktop，再重新执行集成测试。

### Step 20.4 跑全量测试

执行：

```bash
dotnet test
```

验收：

- 所有测试通过。
- `AuthApiTests` 的登录和当前用户测试通过。
- 旧的 `DiagnosticsApiTests` 仍通过。

## 21. 手动验证登录

### Step 21.1 启动 API

执行：

```bash
dotnet run --project src/Api/Api.csproj
```

终端会显示监听地址，例如：

```text
Now listening on: https://localhost:7001
Now listening on: http://localhost:5063
```

下面命令里的端口以你的实际输出为准。

### Step 21.2 调用登录接口

新开一个终端，执行：

```bash
curl -s -X POST http://localhost:5063/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"account":"13800000001","password":"Meta@123456"}'
```

你应该看到类似：

```json
{
  "success": true,
  "code": "OK",
  "message": "success",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "expiresAt": "2026-08-18T12:00:00+00:00",
    "user": {
      "id": 1,
      "userId": "13800000001",
      "mobile": "13800000001",
      "name": "meta-admin",
      "role": 1
    }
  },
  "requestId": "..."
}
```

### Step 21.3 带 token 访问当前用户

为了方便，可以先把 token 复制到变量：

```bash
TOKEN='<上一步返回的 accessToken>'
```

然后执行：

```bash
curl -s http://localhost:5063/api/auth/user \
  -H "Authorization: Bearer $TOKEN"
```

你应该看到：

```json
{
  "success": true,
  "code": "OK",
  "message": "success",
  "data": {
    "id": 1,
    "userId": "13800000001",
    "email": "admin@example.com",
    "name": "meta-admin",
    "realName": "Meta Admin",
    "mobile": "13800000001",
    "role": 1,
    "status": 1
  },
  "requestId": "..."
}
```

### Step 21.4 不带 token 验证 401

执行：

```bash
curl -i http://localhost:5063/api/auth/user
```

你应该看到状态码：

```text
HTTP/1.1 401 Unauthorized
```

响应体类似：

```json
{
  "success": false,
  "code": "UNAUTHORIZED",
  "message": "请先登录",
  "data": null,
  "requestId": "..."
}
```

## 22. 常见问题

### 22.1 登录一直提示账号或密码错误

先检查种子数据有没有写入密码哈希：

```bash
dotnet ef database update \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```

如果本地库里早就有 `users` 数据，seed 可能因为“已有用户”而跳过。学习阶段可以先 drop，再跑种子脚本：

```bash
dotnet ef database drop \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj

./scripts/seed-dev.sh
```

不要把数据库里的 `password_hash` 改成明文密码。

### 22.1.1 登录成功却返回 500，日志里是 InvalidCastException

现象：

```text
Unable to cast object of type 'Api.Responses.ApiResponse`1[Application.Auth.LoginResponse]'
to type 'Application.Auth.LoginResponse'.
```

原因：Day 02 的 `ApiResponseFilter` 包装了 `Value`，但没有同步 `DeclaredType`。`AuthController` 返回 `Task<LoginResponse>` 时会触发这个问题。

处理：打开 `src/Api/Responses/ApiResponseFilter.cs`，在赋值 `objectResult.Value` 之后加上：

```csharp
objectResult.DeclaredType = responseType;
```

改完后重启 `Api`，再试登录。

### 22.2 token 签发成功，但访问接口还是 401

检查顺序：

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

`UseAuthentication()` 必须在前。

再检查 `JwtOptions`：

- 签发 token 用的 `Issuer` 是否和校验配置一致。
- 签发 token 用的 `Audience` 是否和校验配置一致。
- `SigningKey` 是否一致。
- `SigningKey` 长度是否至少 32。

### 22.3 测试里 401 没有统一响应体

检查 `Program.cs` 是否还有：

```csharp
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationResultHandler>();
```

再检查 `AuthorizationResultHandler` 是否处理了 `authorizeResult.Challenged`。

JWT Bearer 校验失败会触发 challenge；如果没有自定义 result handler，ASP.NET Core 默认可能只返回空 401。

### 22.4 过期时间测试不稳定

不要真的等待几分钟来测试 token 过期。Day 05 的集成测试直接用同一组测试 issuer、audience、signing key 生成一个 `expires` 已经早于当前时间 10 分钟的 token。

这样能稳定验证 `ValidateLifetime = true`，也不会为了测试把生产配置里的 `AccessTokenMinutes` 改得很奇怪。

后续如果要精确测试签发时间，可以把 `JwtTokenService` 改成依赖 `TimeProvider`，再在单元测试里注入假时间。

### 22.5 真的企业项目会不会只用本地账号密码

不一定。企业内部系统常见组合是：

- 本地账号密码：适合学习认证闭环、内部管理后台、低集成成本系统。
- SSO/OIDC：适合接入统一身份平台。
- 外部权限中心 token 校验：适合已有集团统一网关的系统。

今天选本地账号密码 + JWT，是为了完整学习标准 .NET 流程。以后如果切到 SSO，也仍然会复用 `[Authorize]`、claims、当前用户访问器这些基础设施。

### 22.6 `Application` 是命名空间，不能当类型用

现象：

```text
error CS0118: “Application”是 命名空间，但此处被当做 类型 来使用
```

原因：

应用层根命名空间是 `Application`，领域实体类也叫 `Application`。一旦出现 `namespace Application.Auth`，编译器会把单独的 `Application` 当成命名空间。

处理：

不要改实体类名。在引用这个实体的文件顶部加别名：

```csharp
using AppEntity = Domain.Entities.Application;
```

然后把 `DbSet<Application>`、`IEntityTypeConfiguration<Application>`、`new Application`、`typeof(Application)` 改成 `AppEntity`。`entity.Application` 这种导航属性名可以保留，那是属性名，不是类型名。

## 23. 今天的提交建议

完成后先看改动：

```bash
git status --short
```

建议至少包含这些文件：

```text
src/Api/Api.csproj
src/Api/Auth/AuthenticationRegistration.cs
src/Api/Auth/CurrentUserAccessor.cs
src/Api/Auth/JwtBearerOptionsSetup.cs
src/Api/Auth/JwtClaimTypes.cs
src/Api/Auth/JwtTokenService.cs
src/Api/Configuration/JwtOptions.cs
src/Api/Configuration/OptionsRegistration.cs
src/Api/Controllers/AuthController.cs
src/Api/Middleware/ExceptionHandlingMiddleware.cs
src/Api/Program.cs
src/Api/appsettings.Development.json
src/Application/ApplicationRegistration.cs
src/Application/Auth/*
src/Domain/Entities/User.cs
src/Infrastructure/Auth/PasswordHashService.cs
src/Infrastructure/Auth/UserCredentialRepository.cs
src/Infrastructure/Persistence/Configurations/UserConfiguration.cs
src/Infrastructure/Persistence/Migrations/*AddUserPasswordHash*
src/Infrastructure/Persistence/PersistenceRegistration.cs
src/Infrastructure/Persistence/SeedData/DevelopmentSeedData.cs
tests/IntegrationTests/AuthApiTests.cs
tests/IntegrationTests/Support/TestEnvironmentFixture.cs
tests/UnitTests/Auth/CurrentUserAccessorTests.cs
tests/UnitTests/OptionsBindingTests.cs
```

执行：

```bash
git add src/Api \
  src/Application \
  src/Domain/Entities/User.cs \
  src/Infrastructure \
  tests/IntegrationTests \
  tests/UnitTests

git commit -m "feat: add local jwt authentication"
```

## 24. 今天你应该理解的概念

### 24.1 Authentication 和 Authorization 不一样

Authentication 是“你是谁”。JWT Bearer handler 校验 token 签名、过期时间、issuer、audience，然后构建 `HttpContext.User`。

Authorization 是“你能不能访问”。`[Authorize]` 会检查当前请求是否已经认证，后续也可以检查角色、策略和资源权限。

### 24.2 JWT 是签名令牌，不是加密令牌

普通 JWT 的 payload 可以被任何拿到 token 的人 base64 解码看到。所以不要把密码、密钥、身份证号这类敏感信息放进 JWT。

今天的 claims 只放：

- 数据库用户 id。
- `userId`。
- 手机号。
- 昵称。
- 角色值。

### 24.3 密码哈希不是加密

加密通常可以解密，哈希不应该能反推原文。登录时不是把哈希“解开”，而是用同一个哈希算法验证输入密码是否匹配。

不要自己设计密码算法。`PasswordHasher<TUser>` 已经处理了 salt、迭代次数和格式版本。

### 24.4 当前用户访问器是边界

业务代码如果到处写：

```csharp
HttpContext.Request.Headers["Authorization"]
```

会让 HTTP 细节扩散到所有模块。`ICurrentUserAccessor` 把这种细节关在 Api 层，后续业务 Service 只依赖一个清晰接口。

### 24.5 标准管线比手写中间件更重要

手写中间件解析 Bearer token 很容易，但后面会遇到：

- Swagger 认证配置。
- `[Authorize]` 策略。
- 多认证方案。
- 401/403 区分。
- claims transformation。
- 测试 host 行为。

接入 ASP.NET Core 标准认证管线，后续扩展才顺。

### 24.6 类型别名不是重命名实体

`using AppEntity = Domain.Entities.Application;` 只是给类型起一个当前文件里的短名。`Domain` 里的类名、数据库表 `applications`、导航属性 `entity.Application` 都不改。它解决的是 C# 名字查找问题：层名 `Application` 和实体名 `Application` 撞车。

## 25. 晚上复盘

可以按这几个问题写学习笔记：

- 今天学会的 C#/.NET 概念：
- 登录接口为什么不能返回密码哈希：
- `PasswordHasher<User>` 比自己写 SHA256 好在哪里：
- `Issuer`、`Audience`、`SigningKey` 分别解决什么问题：
- `UseAuthentication()` 和 `UseAuthorization()` 的顺序为什么不能反：
- `ClaimsPrincipal` 和 `CurrentUser` 的关系：
- 为什么实体 `Application` 要用别名 `AppEntity`，而不是改类名：
- 今天完成的工程产物：
- 明天风险：

## 参考资料

### 当前项目参考

- `docs/superpowers/plans/dotnet-meta-server-40-days/day-01-step-by-step.md`
- `docs/superpowers/plans/dotnet-meta-server-40-days/day-02-step-by-step.md`
- `docs/superpowers/plans/dotnet-meta-server-40-days/day-03-step-by-step.md`
- `docs/superpowers/plans/dotnet-meta-server-40-days/day-04-step-by-step.md`
- `src/Api/Program.cs`
- `src/Api/Responses/AuthorizationResultHandler.cs`
- `src/Api/Middleware/ExceptionHandlingMiddleware.cs`
- `src/Domain/Entities/User.cs`
- `src/Infrastructure/Persistence/SeedData/DevelopmentSeedData.cs`
- `tests/IntegrationTests/Support/TestEnvironmentFixture.cs`

### 原 NestJS 业务源码参考

- `/Users/fenghe/workspace/devops/meta-server/src/biz/auth/auth.controller.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/core/guards/auth.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/core/decorators/user.decorator.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/core/innerServer/index.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/biz/user/user.service.ts`

### 官方文档

- Microsoft Learn：Authentication overview  
  `https://learn.microsoft.com/en-us/aspnet/core/security/authentication/`
- Microsoft Learn：Configure JWT bearer authentication  
  `https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication`
- Microsoft Learn：Authorize attribute  
  `https://learn.microsoft.com/en-us/aspnet/core/security/authorization/simple`
- Microsoft Learn：Options pattern  
  `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options`
- Microsoft Learn：PasswordHasher<TUser>  
  `https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.passwordhasher-1`
