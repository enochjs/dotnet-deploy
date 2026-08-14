# Day 30 - Page fallback、Swagger 和前端代理联调

## 今日目标

补齐页面 fallback、Swagger 配置和前端代理联调，让 `meta-web` 可以系统性访问 .NET 后端。

## 今天学习的 .NET 点

- 静态文件和 fallback route。
- Swagger 分组和环境开关。
- CORS 配置。
- 前端代理与 Network 调试。

## 实现 Todo

- [ ] 实现 Page fallback 或确认由前端独立部署，不需要后端托管。
- [ ] 非 prod/pre 环境启用 Swagger。
- [ ] 配置 CORS：origin、credentials、methods、headers。
- [ ] 启动原 `meta-web`，代理到 .NET 后端。
- [ ] 逐页打开：用户、配置、需求、迭代、集成发布、流水线、发布、监控。
- [ ] 记录每个失败请求的 URL、method、status、request、response。
- [ ] 修复明显 CORS、字段名、响应包装问题。
- [ ] 形成前端联调问题清单。

## 验收标准

- 前端能通过代理访问 .NET 后端。
- Swagger 在开发环境可用。
- CORS 不阻塞登录态请求。
- 每个前端失败请求都有归因。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
