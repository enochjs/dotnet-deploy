# Day 02 - 配置、日志、异常和统一响应

## 今日目标

把 API 的基础体验搭好：配置可管理、日志能追踪、异常格式统一、响应结构兼容前端。

## 今天学习的 .NET 点

- `appsettings.json`、环境变量、User Secrets。
- Options Pattern：`IOptions<T>`、`IOptionsSnapshot<T>`。
- Middleware、Exception Handler、Action Filter 的区别。
- Serilog 或 `ILogger<T>` 的结构化日志。

## 实现 Todo

- [ ] 设计配置类：Postgres、Redis、Git、DingTalk、OSS、InnerServer、Monitor、Logger。
- [ ] 添加 `appsettings.Development.json`，只放本地非敏感默认值。
- [ ] 设计生产环境变量命名规范。
- [ ] 实现统一业务异常类型，例如 `BusinessException`。
- [ ] 实现统一错误响应格式，覆盖 401、400、业务异常、未知异常。
- [ ] 实现统一成功响应包装，兼容原 `TransformInterceptor` 的前端预期。
- [ ] 实现分页模型：`pageIndex`、`pageSize`、`totalCount`、`items`。
- [ ] 加 requestId，并在日志里记录 method、path、status、elapsedMs。
- [ ] 写配置绑定测试和异常响应测试。

## 验收标准

- 缺少关键配置时能尽早失败，并有明确错误。
- 业务异常返回稳定结构。
- 成功响应和分页响应字段名符合原前端习惯。
- 日志中能看到 requestId。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
