# Day 02 Step-by-Step - 配置、日志、异常和统一响应

这份文档是 `day-02.md` 的跟做版。Day 02 内容比 Day 01 难很多，因为它开始碰 ASP.NET Core 的“基础设施”：配置从哪里来、请求怎么经过中间件、异常怎么变成前端能识别的 JSON、日志里怎么带 requestId。

你可以先不用把每个概念都理解透。今天的目标是照着做出一套可运行的 API 基础能力，并知道每块代码大概负责什么。

## 0. 今天最终要得到什么

完成后，你会在 Day 01 的项目上新增这些能力：

- 配置类：Postgres、Redis、Git、DingTalk、OSS、InnerServer、Monitor、Logger。
- `appsettings.Development.json` 里有本地非敏感默认值。
- 生产配置使用统一的环境变量命名规则。
- 业务异常 `BusinessException`。
- 统一错误响应：能覆盖 400、401、业务异常、未知异常。
- 统一成功响应：Controller 返回的数据会包成固定结构。
- 分页响应模型：`pageIndex`、`pageSize`、`totalCount`、`items`。
- 每个请求有 `requestId`，日志里能看到 method、path、status、elapsedMs。
- Serilog 接管日志输出。
- 配置绑定测试、异常响应测试、成功响应测试可以通过。

你最后需要确认三件事：

- `dotnet build` 成功。
- `dotnet test` 成功。
- 启动 API 后，访问测试接口能看到统一响应结构，日志里能看到 requestId。

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

如果不是这个路径，先不要继续，重新执行上面的 `cd` 命令。

### Step 1.2 确认 Day 01 是干净的

执行：

```bash
dotnet build
dotnet test
```

验收：

- `dotnet build` 能看到 `Build succeeded.`
- `dotnet test` 能看到 `Passed!`

如果 Day 01 还没通过，先回到 `day-01-step-by-step.md` 修好。Day 02 会改请求管线，如果基础项目不干净，后面会很难判断错误来自哪里。

## 2. 安装今天需要的 NuGet 包

### Step 2.1 给 Api 添加 Serilog 和 Options 验证包

执行：

```bash
dotnet add src/Api/Api.csproj package Serilog.AspNetCore
dotnet add src/Api/Api.csproj package Serilog.Sinks.Console
dotnet add src/Api/Api.csproj package Serilog.Settings.Configuration
dotnet add src/Api/Api.csproj package Microsoft.Extensions.Options.DataAnnotations
```

这些包先这样理解：

- `Serilog.AspNetCore`：让 ASP.NET Core 使用 Serilog。
- `Serilog.Sinks.Console`：把日志输出到终端。
- `Serilog.Settings.Configuration`：让 Serilog 能读取 `appsettings.json`。
- `Microsoft.Extensions.Options.DataAnnotations`：让配置类可以用 `[Required]` 这类特性做校验。

验收：

```bash
dotnet restore
```

成功时不会有红色错误。

### Step 2.2 如果包版本不兼容

如果你的项目是 `net10.0`，通常直接安装最新版即可。

如果提示类似：

```text
Package ... is not compatible with net10.0
```

先执行：

```bash
dotnet --version
```

确认本机 SDK 版本。然后再重新安装与当前 SDK 更匹配的包版本。初学阶段可以先不指定版本，让 NuGet 自动选：

```bash
dotnet add src/Api/Api.csproj package Serilog.AspNetCore
```

## 3. 设计配置文件结构

### Step 3.1 先理解配置来源

ASP.NET Core 默认会从多个地方读配置。常用顺序可以先记成：

```text
appsettings.json
appsettings.Development.json
User Secrets
环境变量
命令行参数
```

后面的来源可以覆盖前面的来源。

例如：

- `appsettings.json` 放通用默认值。
- `appsettings.Development.json` 放本地开发默认值。
- User Secrets 放本机敏感值。
- 环境变量放服务器上的生产配置。

今天的原则：

- 配置结构可以提交到 git。
- 真实密码、token、accessKey 不提交到 git。
- 缺少关键配置时，程序启动时尽早失败。

## 4. 创建配置类

### Step 4.1 创建目录

执行：

```bash
mkdir -p src/Api/Configuration
```

### Step 4.2 创建 `PostgresOptions.cs`

创建文件：

```text
src/Api/Configuration/PostgresOptions.cs
```

填入：

```csharp
using System.ComponentModel.DataAnnotations;

namespace Api.Configuration;

public sealed class PostgresOptions
{
    public const string SectionName = "Postgres";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;
}
```

你可以先这样理解：

- `SectionName = "Postgres"` 对应配置文件里的 `"Postgres"` 节点。
- `[Required]` 表示这个配置不能为空。
- `init` 表示对象创建时能赋值，创建后不建议再改。

### Step 4.3 创建 `RedisOptions.cs`

创建文件：

```text
src/Api/Configuration/RedisOptions.cs
```

填入：

```csharp
using System.ComponentModel.DataAnnotations;

namespace Api.Configuration;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    [Required]
    public string ConnectionString { get; init; } = string.Empty;
}
```

### Step 4.4 创建 `GitOptions.cs`

创建文件：

```text
src/Api/Configuration/GitOptions.cs
```

填入：

```csharp
using System.ComponentModel.DataAnnotations;

namespace Api.Configuration;

public sealed class GitOptions
{
    public const string SectionName = "Git";

    [Required]
    public string RepositoryRoot { get; init; } = string.Empty;

    public string DefaultBranch { get; init; } = "main";
}
```

### Step 4.5 创建 `DingTalkOptions.cs`

创建文件：

```text
src/Api/Configuration/DingTalkOptions.cs
```

填入：

```csharp
using System.ComponentModel.DataAnnotations;

namespace Api.Configuration;

public sealed class DingTalkOptions
{
    public const string SectionName = "DingTalk";

    [Required]
    public string BaseUrl { get; init; } = string.Empty;

    public string AppKey { get; init; } = string.Empty;

    public string AppSecret { get; init; } = string.Empty;
}
```

`AppSecret` 是敏感配置。本地可以先留空，真正调用钉钉接口前再通过 User Secrets 或环境变量配置。

### Step 4.6 创建 `OssOptions.cs`

创建文件：

```text
src/Api/Configuration/OssOptions.cs
```

填入：

```csharp
using System.ComponentModel.DataAnnotations;

namespace Api.Configuration;

public sealed class OssOptions
{
    public const string SectionName = "OSS";

    [Required]
    public string Endpoint { get; init; } = string.Empty;

    [Required]
    public string BucketName { get; init; } = string.Empty;

    public string AccessKeyId { get; init; } = string.Empty;

    public string AccessKeySecret { get; init; } = string.Empty;
}
```

### Step 4.7 创建 `InnerServerOptions.cs`

创建文件：

```text
src/Api/Configuration/InnerServerOptions.cs
```

填入：

```csharp
using System.ComponentModel.DataAnnotations;

namespace Api.Configuration;

public sealed class InnerServerOptions
{
    public const string SectionName = "InnerServer";

    [Required]
    public string BaseUrl { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 30;
}
```

### Step 4.8 创建 `MonitorOptions.cs`

创建文件：

```text
src/Api/Configuration/MonitorOptions.cs
```

填入：

```csharp
namespace Api.Configuration;

public sealed class MonitorOptions
{
    public const string SectionName = "Monitor";

    public bool Enabled { get; init; } = true;

    public int SlowRequestThresholdMs { get; init; } = 1000;
}
```

### Step 4.9 创建 `LoggerOptions.cs`

创建文件：

```text
src/Api/Configuration/LoggerOptions.cs
```

填入：

```csharp
namespace Api.Configuration;

public sealed class LoggerOptions
{
    public const string SectionName = "Logger";

    public bool IncludeRequestBody { get; init; }

    public bool IncludeResponseBody { get; init; }
}
```

今天先不真的打印请求体和响应体，因为那会涉及隐私、性能和流读取。先把开关设计出来，后面需要时再实现。

## 5. 注册并校验配置类

### Step 5.1 创建配置注册扩展

创建文件：

```text
src/Api/Configuration/OptionsRegistration.cs
```

填入：

```csharp
using Microsoft.Extensions.Options;

namespace Api.Configuration;

public static class OptionsRegistration
{
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

        return services;
    }

    private static OptionsBuilder<TOptions> AddValidatedOptions<TOptions>(
        this IServiceCollection services,
        string sectionName)
        where TOptions : class
    {
        return services
            .AddOptions<TOptions>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
```

你可以先这样理解：

- `BindConfiguration(sectionName)`：把配置文件里的某个节点绑定到 C# 类。
- `ValidateDataAnnotations()`：执行 `[Required]` 等校验。
- `ValidateOnStart()`：程序启动时就校验，而不是等第一次使用配置时才报错。

### Step 5.2 修改 `Program.cs` 注册配置

打开：

```text
src/Api/Program.cs
```

在文件顶部加：

```csharp
using Api.Configuration;
```

在 `builder.Services.AddSwaggerGen();` 后面加：

```csharp
builder.Services.AddApplicationOptions();
```

修改后，`Program.cs` 前半段大概是：

```csharp
using Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationOptions();
```

### Step 5.3 先不要立刻 build

现在如果直接 `dotnet build`，编译应该能通过。

但如果你执行 `dotnet run`，可能会因为配置缺失启动失败。这个失败是我们想要的能力：缺关键配置时尽早失败。

下一步先补配置文件。

## 6. 编写本地开发配置

### Step 6.1 修改 `appsettings.json`

打开：

```text
src/Api/appsettings.json
```

替换为：

```json
{
  "AllowedHosts": "*",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "Enrich": [ "FromLogContext" ],
    "WriteTo": [
      {
        "Name": "Console"
      }
    ]
  }
}
```

`appsettings.json` 只放通用配置，不放本地数据库地址，也不放密码。

### Step 6.2 修改 `appsettings.Development.json`

打开：

```text
src/Api/appsettings.Development.json
```

替换为：

```json
{
  "Postgres": {
    "ConnectionString": "Host=localhost;Port=5432;Database=dotnet_meta_server;Username=postgres;Password=postgres"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Git": {
    "RepositoryRoot": "/tmp/dotnet-meta-server/repositories",
    "DefaultBranch": "main"
  },
  "DingTalk": {
    "BaseUrl": "https://api.dingtalk.com",
    "AppKey": "",
    "AppSecret": ""
  },
  "OSS": {
    "Endpoint": "https://oss-cn-hangzhou.aliyuncs.com",
    "BucketName": "dotnet-meta-server-dev",
    "AccessKeyId": "",
    "AccessKeySecret": ""
  },
  "InnerServer": {
    "BaseUrl": "http://localhost:5000",
    "TimeoutSeconds": 30
  },
  "Monitor": {
    "Enabled": true,
    "SlowRequestThresholdMs": 1000
  },
  "Logger": {
    "IncludeRequestBody": false,
    "IncludeResponseBody": false
  }
}
```

注意：

- 这里的数据库、Redis、OSS 都只是本地开发默认值。
- `AppSecret`、`AccessKeySecret` 先留空。
- 以后接真实服务时，不要把真实密钥写进这个文件。

### Step 6.3 配置 User Secrets

如果你还没有初始化 User Secrets，执行：

```bash
dotnet user-secrets init --project src/Api/Api.csproj
```

以后如果需要配置本机密钥，可以这样写：

```bash
dotnet user-secrets set "DingTalk:AppSecret" "你的本机密钥" --project src/Api/Api.csproj
dotnet user-secrets set "OSS:AccessKeySecret" "你的本机密钥" --project src/Api/Api.csproj
```

今天不需要真实密钥，先知道这个用法即可。

### Step 6.4 生产环境变量命名规范

生产环境不要改 `appsettings.Development.json`，而是用环境变量覆盖。

ASP.NET Core 里，层级配置用两个下划线 `__` 表示。

示例：

```bash
Postgres__ConnectionString="Host=prod-db;Port=5432;Database=dotnet_meta_server;Username=app;Password=***"
Redis__ConnectionString="prod-redis:6379,password=***"
Git__RepositoryRoot="/data/repositories"
DingTalk__BaseUrl="https://api.dingtalk.com"
DingTalk__AppKey="***"
DingTalk__AppSecret="***"
OSS__Endpoint="https://oss-cn-hangzhou.aliyuncs.com"
OSS__BucketName="dotnet-meta-server-prod"
OSS__AccessKeyId="***"
OSS__AccessKeySecret="***"
InnerServer__BaseUrl="http://inner-server"
InnerServer__TimeoutSeconds="30"
Monitor__Enabled="true"
Monitor__SlowRequestThresholdMs="1000"
Logger__IncludeRequestBody="false"
Logger__IncludeResponseBody="false"
```

你可以先记住：

```text
配置文件：Postgres:ConnectionString
环境变量：Postgres__ConnectionString
```

### Step 6.5 执行 build

执行：

```bash
dotnet build
```

验收：

- 能看到 `Build succeeded.`

如果报 `OptionsBuilder` 找不到，一般是命名空间缺失。确认 `OptionsRegistration.cs` 顶部有：

```csharp
using Microsoft.Extensions.Options;
```

如果报 `IServiceCollection` 找不到，确认项目开启了 `ImplicitUsings`。Day 01 的 `Directory.Build.props` 里应该已经有：

```xml
<ImplicitUsings>enable</ImplicitUsings>
```

## 7. 创建统一响应模型

### Step 7.1 创建目录

执行：

```bash
mkdir -p src/Api/Responses
```

### Step 7.2 创建 `ApiResponse.cs`

创建文件：

```text
src/Api/Responses/ApiResponse.cs
```

填入：

```csharp
namespace Api.Responses;

public sealed record ApiResponse<T>(
    bool Success,
    string Code,
    string Message,
    T? Data,
    string RequestId)
{
    public static ApiResponse<T> Ok(T? data, string requestId)
    {
        return new ApiResponse<T>(true, "OK", "success", data, requestId);
    }

    public static ApiResponse<T> Fail(string code, string message, string requestId)
    {
        return new ApiResponse<T>(false, code, message, default, requestId);
    }
}
```

前端最后会看到类似：

```json
{
  "success": true,
  "code": "OK",
  "message": "success",
  "data": {
    "status": "ok"
  },
  "requestId": "0HN..."
}
```

### Step 7.3 创建 `PagedResponse.cs`

创建文件：

```text
src/Api/Responses/PagedResponse.cs
```

填入：

```csharp
namespace Api.Responses;

public sealed record PagedResponse<T>(
    int PageIndex,
    int PageSize,
    long TotalCount,
    IReadOnlyList<T> Items);
```

你可以先这样理解：

- `ApiResponse<T>` 是最外层包装。
- `PagedResponse<T>` 是分页数据本身。
- 分页接口最终会返回 `ApiResponse<PagedResponse<T>>`。

## 8. 实现业务异常

### Step 8.1 创建目录

执行：

```bash
mkdir -p src/Api/Exceptions
```

### Step 8.2 创建 `BusinessException.cs`

创建文件：

```text
src/Api/Exceptions/BusinessException.cs
```

填入：

```csharp
namespace Api.Exceptions;

public sealed class BusinessException : Exception
{
    public BusinessException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
```

你可以先这样理解：

- 业务异常不是程序崩了，而是业务规则不允许。
- 例如“订单状态不能取消”“用户余额不足”“重复提交”。
- 它应该返回稳定的错误码和错误消息。

## 9. 实现 requestId 上下文

### Step 9.1 创建 `RequestIdProvider.cs`

创建文件：

```text
src/Api/Responses/RequestIdProvider.cs
```

填入：

```csharp
namespace Api.Responses;

public static class RequestIdProvider
{
    public const string HeaderName = "X-Request-Id";

    public static string Get(HttpContext context)
    {
        if (context.Items.TryGetValue(HeaderName, out var value) &&
            value is string requestId &&
            !string.IsNullOrWhiteSpace(requestId))
        {
            return requestId;
        }

        return context.TraceIdentifier;
    }
}
```

这个小工具让中间件、Filter、测试都能用同一套 requestId 读取逻辑。

## 10. 实现请求日志中间件

### Step 10.1 创建目录

执行：

```bash
mkdir -p src/Api/Middleware
```

### Step 10.2 创建 `RequestLoggingMiddleware.cs`

创建文件：

```text
src/Api/Middleware/RequestLoggingMiddleware.cs
```

填入：

```csharp
using System.Diagnostics;
using Api.Responses;
using Serilog.Context;

namespace Api.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = GetOrCreateRequestId(context);
        context.Items[RequestIdProvider.HeaderName] = requestId;
        context.Response.Headers[RequestIdProvider.HeaderName] = requestId;

        using var requestIdScope = LogContext.PushProperty("RequestId", requestId);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }

    private static string GetOrCreateRequestId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(RequestIdProvider.HeaderName, out var values))
        {
            var incomingRequestId = values.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(incomingRequestId))
            {
                return incomingRequestId;
            }
        }

        return context.TraceIdentifier;
    }
}
```

你可以先这样理解：

- 中间件像一层门，请求进来会经过它。
- `_next(context)` 表示把请求交给下一层。
- `finally` 表示不管请求成功还是失败，都记录日志。
- `LogContext.PushProperty("RequestId", requestId)` 会把 requestId 放进 Serilog 日志上下文。

## 11. 实现统一异常中间件

### Step 11.1 创建 `ExceptionHandlingMiddleware.cs`

创建文件：

```text
src/Api/Middleware/ExceptionHandlingMiddleware.cs
```

填入：

```csharp
using System.Net;
using Api.Exceptions;
using Api.Responses;

namespace Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
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
                exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception");

            await WriteErrorAsync(
                context,
                HttpStatusCode.InternalServerError,
                "INTERNAL_SERVER_ERROR",
                "服务器开小差了，请稍后再试");
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string code,
        string message)
    {
        if (context.Response.HasStarted)
        {
            throw new InvalidOperationException("The response has already started.");
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var requestId = RequestIdProvider.Get(context);
        var response = ApiResponse<object>.Fail(code, message, requestId);

        await context.Response.WriteAsJsonAsync(response);
    }
}
```

注意这里：

- `BusinessException` 返回 400，错误码用业务自己传进来的 code。
- 未知异常返回 500，但不要把真实异常堆栈返回给前端。
- 真实异常会进日志，方便后端排查。

## 12. 实现 401 和 400 的统一格式

### Step 12.1 创建 `AuthorizationResultHandler.cs`

今天还没有真正接登录鉴权，但可以先准备 401 的统一输出。

创建文件：

```text
src/Api/Responses/AuthorizationResultHandler.cs
```

填入：

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Api.Responses;

public sealed class AuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Fail(
                "UNAUTHORIZED",
                "请先登录",
                RequestIdProvider.Get(context));

            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
```

### Step 12.2 400 参数错误先在 `Program.cs` 里处理

打开：

```text
src/Api/Program.cs
```

把：

```csharp
builder.Services.AddControllers();
```

改成：

```csharp
builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ApiResponseFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var requestId = RequestIdProvider.Get(context.HttpContext);
            var errors = context.ModelState
                .Where(item => item.Value?.Errors.Count > 0)
                .ToDictionary(
                    item => item.Key,
                    item => item.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

            var response = ApiResponse<object>.Fail(
                "VALIDATION_ERROR",
                "请求参数不正确",
                requestId);

            return new BadRequestObjectResult(new
            {
                response.Success,
                response.Code,
                response.Message,
                Data = errors,
                response.RequestId
            });
        };
    });
```

然后在文件顶部补这些 using：

```csharp
using Api.Responses;
using Microsoft.AspNetCore.Mvc;
```

这里先看懂一件事：

- Controller 参数验证失败时，ASP.NET Core 默认会返回自己的 400 格式。
- 我们把它改成统一格式，前端就不用同时兼容很多种错误结构。

## 13. 实现统一成功响应包装

### Step 13.1 创建 `ApiResponseFilter.cs`

创建文件：

```text
src/Api/Responses/ApiResponseFilter.cs
```

填入：

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Responses;

public sealed class ApiResponseFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        var statusCode = context.HttpContext.Response.StatusCode;

        if (context.Result is ObjectResult objectResult &&
            IsSuccessStatusCode(objectResult.StatusCode ?? statusCode) &&
            objectResult.Value is not null &&
            !IsApiResponse(objectResult.Value.GetType()))
        {
            var requestId = RequestIdProvider.Get(context.HttpContext);
            var valueType = objectResult.Value.GetType();
            var responseType = typeof(ApiResponse<>).MakeGenericType(valueType);

            objectResult.Value = Activator.CreateInstance(
                responseType,
                true,
                "OK",
                "success",
                objectResult.Value,
                requestId);
        }

        await next();
    }

    private static bool IsSuccessStatusCode(int statusCode)
    {
        return statusCode >= 200 && statusCode < 300;
    }

    private static bool IsApiResponse(Type type)
    {
        return type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(ApiResponse<>);
    }
}
```

你可以先这样理解：

- Action Filter 可以在 Controller 返回结果前后做处理。
- 这里的处理是：如果 Controller 返回普通对象，就自动包一层 `ApiResponse<T>`。
- 只包装 2xx 成功响应，400、401、500 这类错误响应不在这里处理。
- 如果本来已经是 `ApiResponse<T>`，就不要重复包装。

### Step 13.2 确认 `Program.cs` 已经注册 Filter

如果你已经完成 Step 12.2，`Program.cs` 里应该有：

```csharp
options.Filters.Add<ApiResponseFilter>();
```

如果没有，补上。

## 14. 修改 `Program.cs` 接入中间件和 Serilog

### Step 14.1 替换 `Program.cs`

为了减少拼错位置，这一步建议直接把整个文件替换成下面版本：

```csharp
using Api.Configuration;
using Api.Middleware;
using Api.Responses;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "DotnetMetaServer")
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{RequestId}] {Message:lj}{NewLine}{Exception}");
});

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ApiResponseFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var requestId = RequestIdProvider.Get(context.HttpContext);
            var errors = context.ModelState
                .Where(item => item.Value?.Errors.Count > 0)
                .ToDictionary(
                    item => item.Key,
                    item => item.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

            var response = ApiResponse<object>.Fail(
                "VALIDATION_ERROR",
                "请求参数不正确",
                requestId);

            return new BadRequestObjectResult(new
            {
                response.Success,
                response.Code,
                response.Message,
                Data = errors,
                response.RequestId
            });
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();
builder.Services.AddApplicationOptions();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationResultHandler>();

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

public partial class Program { }
```

这里顺序很重要：

- `RequestLoggingMiddleware` 放前面，这样成功和失败请求都会记录日志。
- `ExceptionHandlingMiddleware` 放在业务处理前面，这样后面的异常能被它捕获。
- `UseAuthorization()` 放在 `MapControllers()` 前。

### Step 14.2 执行 build

执行：

```bash
dotnet build
```

验收：

- 能看到 `Build succeeded.`

如果报 `ApiResponseFilter` 找不到，确认：

- 文件路径是 `src/Api/Responses/ApiResponseFilter.cs`。
- `Program.cs` 顶部有 `using Api.Responses;`。

如果报 `UseSerilog` 找不到，确认已经执行：

```bash
dotnet add src/Api/Api.csproj package Serilog.AspNetCore
```

## 15. 添加测试用 Controller

真实业务接口还没开始写。为了验证统一响应和异常格式，我们先加一个只用于学习的 Controller。

### Step 15.1 创建目录

执行：

```bash
mkdir -p src/Api/Controllers
```

### Step 15.2 创建 `DiagnosticsController.cs`

创建文件：

```text
src/Api/Controllers/DiagnosticsController.cs
```

填入：

```csharp
using System.ComponentModel.DataAnnotations;
using Api.Exceptions;
using Api.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/diagnostics")]
public sealed class DiagnosticsController : ControllerBase
{
    [HttpGet("success")]
    public ActionResult<object> Success()
    {
        return Ok(new
        {
            status = "ok"
        });
    }

    [HttpGet("business-error")]
    public ActionResult BusinessError()
    {
        throw new BusinessException("DEMO_BUSINESS_ERROR", "这是一个业务异常示例");
    }

    [HttpGet("server-error")]
    public ActionResult ServerError()
    {
        throw new InvalidOperationException("This is a demo exception.");
    }

    [Authorize]
    [HttpGet("secure")]
    public ActionResult<object> Secure()
    {
        return Ok(new
        {
            status = "secure"
        });
    }

    [HttpGet("paged")]
    public ActionResult<PagedResponse<string>> Paged()
    {
        var response = new PagedResponse<string>(
            PageIndex: 1,
            PageSize: 10,
            TotalCount: 2,
            Items: ["alpha", "beta"]);

        return Ok(response);
    }

    [HttpPost("validation")]
    public ActionResult<object> Validation(ValidationRequest request)
    {
        return Ok(new
        {
            request.Name
        });
    }
}

public sealed record ValidationRequest(
  [Required] string Name
);
```

这个 Controller 只是 Day 02 的练习入口。以后有正式业务接口后，可以删除它或只在开发环境启用。

### Step 15.3 执行 build

执行：

```bash
dotnet build
```

验收：

- 能看到 `Build succeeded.`

如果 `Items: ["alpha", "beta"]` 报语法错误，说明你的 C# 版本不支持集合表达式。改成：

```csharp
Items: new[] { "alpha", "beta" });
```

## 16. 手动启动验证

### Step 16.1 启动 API

执行：

```bash
dotnet run --project src/Api/Api.csproj
```

启动后，终端会显示类似：

```text
Now listening on: http://localhost:****
Now listening on: https://localhost:****
```

下面用 `http://localhost:5063` 表示你终端里实际看到的地址。

### Step 16.2 验证成功响应

新开一个终端，执行：

```bash
curl http://localhost:5063/api/diagnostics/success
```

你应该看到类似：

```json
{
  "success": true,
  "code": "OK",
  "message": "success",
  "data": {
    "status": "ok"
  },
  "requestId": "0H..."
}
```

### Step 16.3 验证业务异常

执行：

```bash
curl -i http://localhost:5063/api/diagnostics/business-error
```

你应该看到：

```text
HTTP/1.1 400 Bad Request
```

响应体类似：

```json
{
  "success": false,
  "code": "DEMO_BUSINESS_ERROR",
  "message": "这是一个业务异常示例",
  "data": null,
  "requestId": "0H..."
}
```

### Step 16.4 验证未知异常

执行：

```bash
curl -i http://localhost:5063/api/diagnostics/server-error
```

你应该看到：

```text
HTTP/1.1 500 Internal Server Error
```

响应体类似：

```json
{
  "success": false,
  "code": "INTERNAL_SERVER_ERROR",
  "message": "服务器开小差了，请稍后再试",
  "data": null,
  "requestId": "0H..."
}
```

同时，运行 API 的终端里应该能看到异常日志。

### Step 16.5 验证 401 未登录响应

执行：

```bash
curl -i http://localhost:5063/api/diagnostics/secure
```

你应该看到：

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
  "requestId": "0H..."
}
```

今天还没有真正接登录系统，所以这个接口一定会返回 401。它只是用来确认 401 格式已经统一。

### Step 16.6 验证分页响应

执行：

```bash
curl http://localhost:5063/api/diagnostics/paged
```

你应该看到类似：

```json
{
  "success": true,
  "code": "OK",
  "message": "success",
  "data": {
    "pageIndex": 1,
    "pageSize": 10,
    "totalCount": 2,
    "items": [
      "alpha",
      "beta"
    ]
  },
  "requestId": "0H..."
}
```

### Step 16.7 验证参数错误

执行：

```bash
curl -i \
  -H "Content-Type: application/json" \
  -d "{}" \
  http://localhost:5063/api/diagnostics/validation
```

你应该看到：

```text
HTTP/1.1 400 Bad Request
```

响应体里应该包含：

```json
{
  "success": false,
  "code": "VALIDATION_ERROR",
  "message": "请求参数不正确"
}
```

### Step 16.8 验证 requestId

执行：

```bash
curl -i \
  -H "X-Request-Id: demo-request-001" \
  http://localhost:5063/api/diagnostics/success
```

你应该看到两处 `demo-request-001`：

- 响应头：`X-Request-Id: demo-request-001`
- 响应体：`"requestId":"demo-request-001"`

运行 API 的终端日志里也应该能看到这个 requestId。

## 17. 写集成测试

### Step 17.1 创建 `DiagnosticsApiTests.cs`

创建文件：

```text
tests/IntegrationTests/DiagnosticsApiTests.cs
```

填入：

```csharp
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests;

public class DiagnosticsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DiagnosticsApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Success_ReturnsUnifiedResponse()
    {
        var client = _factory.CreateClient();

        var response = await client.GetFromJsonAsync<ApiEnvelope<object>>("/api/diagnostics/success");

        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("OK", response.Code);
        Assert.Equal("success", response.Message);
        Assert.False(string.IsNullOrWhiteSpace(response.RequestId));
    }

    [Fact]
    public async Task BusinessError_ReturnsStableError()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/diagnostics/business-error");
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal("DEMO_BUSINESS_ERROR", body.Code);
        Assert.Equal("这是一个业务异常示例", body.Message);
        Assert.False(string.IsNullOrWhiteSpace(body.RequestId));
    }

    [Fact]
    public async Task ServerError_ReturnsUnifiedError()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/diagnostics/server-error");
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal("INTERNAL_SERVER_ERROR", body.Code);
        Assert.False(string.IsNullOrWhiteSpace(body.RequestId));
    }

    [Fact]
    public async Task SecureEndpoint_ReturnsUnifiedUnauthorizedError()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/diagnostics/secure");
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal("UNAUTHORIZED", body.Code);
        Assert.False(string.IsNullOrWhiteSpace(body.RequestId));
    }

    [Fact]
    public async Task ValidationError_ReturnsUnifiedBadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/diagnostics/validation", new { });
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal("VALIDATION_ERROR", body.Code);
        Assert.False(string.IsNullOrWhiteSpace(body.RequestId));
    }

    [Fact]
    public async Task RequestId_CanBeProvidedByHeader()
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/diagnostics/success");
        request.Headers.Add("X-Request-Id", "test-request-001");

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<object>>();

        Assert.Equal("test-request-001", response.Headers.GetValues("X-Request-Id").Single());
        Assert.NotNull(body);
        Assert.Equal("test-request-001", body.RequestId);
    }

    private sealed record ApiEnvelope<T>(
        bool Success,
        string Code,
        string Message,
        T? Data,
        string RequestId);
}
```

测试里的 `ApiEnvelope<T>` 是为了读取 JSON 响应。它不一定要引用 API 项目的 `ApiResponse<T>`，这样测试更像一个真正的外部调用者。

### Step 17.2 运行集成测试

执行：

```bash
dotnet test tests/IntegrationTests/IntegrationTests.csproj
```

验收：

- 能看到 `Passed!`

如果测试启动时报配置缺失，确认 `appsettings.Development.json` 里有 Step 6.2 的配置。

## 18. 写配置绑定测试

### Step 18.1 让 UnitTests 引用 Api 并添加测试依赖

今天配置类暂时放在 `Api` 项目里，所以单元测试需要引用 `Api`。

执行：

```bash
dotnet add tests/UnitTests/UnitTests.csproj reference src/Api/Api.csproj
dotnet add tests/UnitTests/UnitTests.csproj package Microsoft.Extensions.Configuration
dotnet add tests/UnitTests/UnitTests.csproj package Microsoft.Extensions.Configuration.Binder
dotnet add tests/UnitTests/UnitTests.csproj package Microsoft.Extensions.DependencyInjection
dotnet add tests/UnitTests/UnitTests.csproj package Microsoft.Extensions.Options
dotnet add tests/UnitTests/UnitTests.csproj package Microsoft.Extensions.Options.DataAnnotations
```

这些包是为了让测试项目能自己创建一套内存配置和依赖注入容器。

### Step 18.2 创建 `OptionsBindingTests.cs`

创建文件：

```text
tests/UnitTests/OptionsBindingTests.cs
```

填入：

```csharp
using Api.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace UnitTests;

public class OptionsBindingTests
{
    [Fact]
    public void PostgresOptions_BindsFromConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Postgres:ConnectionString"] = "Host=localhost;Database=test",
                ["Redis:ConnectionString"] = "localhost:6379",
                ["Git:RepositoryRoot"] = "/tmp/repositories",
                ["Git:DefaultBranch"] = "main",
                ["DingTalk:BaseUrl"] = "https://api.dingtalk.com",
                ["OSS:Endpoint"] = "https://oss-cn-hangzhou.aliyuncs.com",
                ["OSS:BucketName"] = "bucket",
                ["InnerServer:BaseUrl"] = "http://localhost:5000",
                ["InnerServer:TimeoutSeconds"] = "30",
                ["Monitor:Enabled"] = "true",
                ["Monitor:SlowRequestThresholdMs"] = "1000",
                ["Logger:IncludeRequestBody"] = "false",
                ["Logger:IncludeResponseBody"] = "false"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddApplicationOptions();

        using var provider = services.BuildServiceProvider(validateScopes: true);
        var options = provider.GetRequiredService<IOptions<PostgresOptions>>().Value;

        Assert.Equal("Host=localhost;Database=test", options.ConnectionString);
    }

    [Fact]
    public void MissingRequiredOptions_FailsValidation()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Redis:ConnectionString"] = "localhost:6379"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddApplicationOptions();

        using var provider = services.BuildServiceProvider(validateScopes: true);

        Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<PostgresOptions>>().Value);
    }
}
```

### Step 18.3 运行单元测试

执行：

```bash
dotnet test tests/UnitTests/UnitTests.csproj
```

验收：

- 能看到 `Passed!`

如果报 `Microsoft.Extensions.Configuration` 找不到，一般是因为 Step 18.1 的包没有装完整。重新执行：

```bash
dotnet add tests/UnitTests/UnitTests.csproj package Microsoft.Extensions.Configuration
dotnet add tests/UnitTests/UnitTests.csproj package Microsoft.Extensions.Configuration.Binder
dotnet add tests/UnitTests/UnitTests.csproj package Microsoft.Extensions.DependencyInjection
dotnet add tests/UnitTests/UnitTests.csproj package Microsoft.Extensions.Options
dotnet add tests/UnitTests/UnitTests.csproj package Microsoft.Extensions.Options.DataAnnotations
```

## 19. 跑完整验证

### Step 19.1 build

执行：

```bash
dotnet build
```

通过标准：

```text
Build succeeded.
```

### Step 19.2 test

执行：

```bash
dotnet test
```

通过标准：

```text
Passed!
```

### Step 19.3 run

执行：

```bash
dotnet run --project src/Api/Api.csproj
```

手动访问：

```text
http://localhost:5063/api/diagnostics/success
http://localhost:5063/api/diagnostics/business-error
http://localhost:5063/api/diagnostics/paged
```

通过标准：

- 成功响应有 `success`、`code`、`message`、`data`、`requestId`。
- 业务异常返回 400。
- 未知异常返回 500。
- 分页响应里有 `pageIndex`、`pageSize`、`totalCount`、`items`。
- 终端日志里有 method、path、status、elapsedMs、requestId。

## 20. 你需要能说出来的概念

完成后，试着用自己的话说下面这段：

```text
appsettings.json 放通用配置，appsettings.Development.json 放本地开发默认值，真实密钥应该用 User Secrets 或环境变量。

Options Pattern 是把配置绑定成 C# 类，代码里不直接到处读字符串配置。

Middleware 是请求管线的一层，适合做日志、异常处理这种所有请求都要经过的事情。

Action Filter 更靠近 Controller，适合包装 Controller 返回值。

BusinessException 表示业务规则不允许，不是程序未知崩溃。

Serilog 是结构化日志库，可以把 requestId、method、path、status、elapsedMs 这些字段稳定打进日志。
```

如果你能说出这段，Day 2 的理解目标就达到了。

## 21. 常见卡点

### 卡点 1：`UseSerilog` 找不到

原因：没有安装 `Serilog.AspNetCore`。

处理：

```bash
dotnet add src/Api/Api.csproj package Serilog.AspNetCore
dotnet restore
dotnet build
```

### 卡点 2：启动时报配置缺失

原因：配置类加了 `[Required]`，但配置文件里没有对应节点。

处理：

- 检查 `src/Api/appsettings.Development.json` 是否包含所有 Day 02 配置节点。
- 检查当前环境是不是 Development。

查看当前启动环境可以看终端输出：

```text
Hosting environment: Development
```

如果不是 Development，可以临时这样启动：

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Api/Api.csproj
```

### 卡点 3：返回结果被包了两层

现象类似：

```json
{
  "success": true,
  "data": {
    "success": true,
    "data": {}
  }
}
```

原因：`ApiResponseFilter` 没有正确判断 `ApiResponse<T>`。

处理：确认 `ApiResponseFilter.cs` 里有：

```csharp
private static bool IsApiResponse(Type type)
{
    return type.IsGenericType &&
        type.GetGenericTypeDefinition() == typeof(ApiResponse<>);
}
```

### 卡点 4：`/health` 没有被统一包装

原因：`/health` 是 Minimal API，不是 Controller，所以不会经过 MVC 的 Action Filter。

今天可以接受这个结果。Day 02 的统一成功响应主要先覆盖 Controller。

如果你一定要让 `/health` 也统一包装，可以把它改成：

```csharp
app.MapGet("/health", (HttpContext context) =>
{
    var data = new { status = "ok" };
    return Results.Ok(ApiResponse<object>.Ok(data, RequestIdProvider.Get(context)));
});
```

但这一步不是今天必须完成的。

### 卡点 5：测试里 JSON 字段都是默认值

原因可能是响应字段名大小写和 record 构造参数不匹配，或者响应不是你以为的结构。

处理：

先把原始响应打印出来：

```csharp
var json = await response.Content.ReadAsStringAsync();
Console.WriteLine(json);
```

然后执行：

```bash
dotnet test --logger "console;verbosity=detailed"
```

看实际 JSON 长什么样。

## 22. 晚上复盘

在这个文件里记录：

```text
docs/learning-notes/day-02-api-foundation.md
```

如果文件不存在，就创建它。可以先写这个模板：

```markdown
# Day 02 - API 基础设施复盘

## 今天学会的 C#/.NET 概念

- appsettings.json / appsettings.Development.json：
- User Secrets：
- 环境变量双下划线：
- Options Pattern：
- Middleware：
- Action Filter：
- BusinessException：
- Serilog：

## 今天完成的工程产物

- 配置类：
- 统一响应：
- 统一异常：
- 请求日志：
- 测试：

## 今天最卡的地方

-

## 明天风险

-
```

## 23. 建议提交

如果你使用 git，确认状态：

```bash
git status --short
```

如果 build 和 test 都通过，可以提交：

```bash
git add .
git commit -m "feat: add api configuration logging and response foundation"
```

提交不是今天学习 .NET 的必要条件，但它能帮你保存一个干净的 Day 2 节点。
