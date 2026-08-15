# NestJS 与 ASP.NET Core 概念类比

| NestJS | ASP.NET Core 更接近的东西 | 主要作用 |
|---|---|---|
| Middleware | Middleware | HTTP 请求管道的通用处理 |
| Interceptor | Action Filter / Result Filter | Controller 执行前后、返回结果处理 |
| Guard | Authorization Policy / Authorization Filter | 身份与权限判断 |
| Exception Filter | MVC Exception Filter | 处理 MVC / Controller 内部异常 |
| Pipe | Model Binding + Validation | 参数转换、绑定、验证 |
| Controller | Controller | 接收请求并调用业务逻辑 |
| Provider / Service | DI Service | 业务逻辑、依赖注入 |

## 大致的请求流程

### NestJS

HTTP Request

→ Middleware  
→ Guard  
→ Interceptor（before）  
→ Pipe  
→ Controller  
→ Interceptor（after）  
→ Exception Filter（发生异常时）  
→ HTTP Response

### ASP.NET Core

HTTP Request

→ Middleware  
→ Authorization  
→ Model Binding  
→ Model Validation  
→ Action Filter（before）  
→ Controller  
→ Action Filter（after）  
→ Result Filter  
→ HTTP Response

## 特别注意

NestJS：

Exception Filter  
≈ ASP.NET Core MVC Exception Filter

但 ASP.NET Core 如果需要处理**整个 HTTP Pipeline 的全局异常**，通常更适合使用：

ExceptionHandlingMiddleware

因为 Middleware 可以覆盖 MVC / Controller 之外发生的异常。


# MvcOptions vs ApiBehaviorOptions

 ApiBehaviorOptions = 修改 [ApiController] 帮你兜底的默认 API 行为。

| 配置位置 | 适合放什么 | 典型场景 | 例子 |
|---|---|---|---|
| `MvcOptions` | Controller / MVC 执行机制相关配置 | 全局 Filter | `options.Filters.Add<ApiResponseFilter>()` |
| `MvcOptions` | Controller / MVC 执行机制相关配置 | 自定义 Model Binder | `options.ModelBinderProviders.Insert(...)` |
| `MvcOptions` | Controller / MVC 执行机制相关配置 | 自定义输入格式 | `options.InputFormatters.Add(...)` |
| `MvcOptions` | Controller / MVC 执行机制相关配置 | 自定义输出格式 | `options.OutputFormatters.Add(...)` |
| `MvcOptions` | Controller / MVC 执行机制相关配置 | 修改 MVC 的参数绑定行为 | 配置 `ModelBindingMessageProvider` |
| `MvcOptions` | Controller / MVC 执行机制相关配置 | 控制是否允许空输入、Formatter 等 MVC 行为 | MVC 基础配置 |
| `ApiBehaviorOptions` | `[ApiController]` 的自动行为 | 参数验证失败时自动返回什么 | `InvalidModelStateResponseFactory` |
| `ApiBehaviorOptions` | `[ApiController]` 的自动行为 | 是否关闭自动 400 | `SuppressModelStateInvalidFilter` |
| `ApiBehaviorOptions` | `[ApiController]` 的自动行为 | 是否关闭自动绑定来源推断 | `SuppressInferBindingSourcesForParameters` |
| `ApiBehaviorOptions` | `[ApiController]` 的自动行为 | 是否关闭自动生成客户端错误详情 | `SuppressMapClientErrors` |
| `ApiBehaviorOptions` | `[ApiController]` 的自动行为 | 自定义 4xx 错误映射 | `ClientErrorMapping` |

## 简单记忆

| 问题 | 用哪个 |
|---|---|
| Controller 执行流程怎么工作？ | `MvcOptions` |
| Filter 怎么配置？ | `MvcOptions` |
| 参数怎么绑定？ | `MvcOptions` |
| 请求/响应格式怎么转换？ | `MvcOptions` |
| `[ApiController]` 自动帮我做的事情怎么改？ | `ApiBehaviorOptions` |
| 参数验证失败自动 400 怎么改？ | `ApiBehaviorOptions` |
| 是否关闭 `[ApiController]` 的某些自动行为？ | `ApiBehaviorOptions` |

## 你当前项目中的对应关系

```csharp
builder.Services
    .AddControllers(mvcOptions =>
    {
        // MVC / Controller 层面的配置
        mvcOptions.Filters.Add<ApiResponseFilter>();
    })
    .ConfigureApiBehaviorOptions(apiOptions =>
    {
        // [ApiController] 自动行为的配置
        apiOptions.InvalidModelStateResponseFactory = context =>
        {
            // 参数校验失败时，自定义 400 Response
        };
    });