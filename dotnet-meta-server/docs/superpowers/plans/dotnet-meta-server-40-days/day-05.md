# Day 05 - 鉴权、当前用户和 InnerServer

## 今日目标

实现鉴权和当前用户获取。这个模块小，但会贯穿所有需要登录的 API。

## 今天学习的 .NET 点

- ASP.NET Core Authentication/Authorization。
- 从 Header 读取 Bearer Token。
- 自定义当前用户上下文。
- Redis 缓存和 HttpClient 调用外部权限中心。

## 实现 Todo

- [ ] 实现 `CurrentUser` 模型：userId、mobile、name、token。
- [ ] 实现 InnerServer typed client：`CheckToken`、`SearchUser`。
- [ ] 实现 Bearer token 解析。
- [ ] 实现 token -> userInfo 的 Redis 缓存。
- [ ] 实现鉴权中间件或认证 handler。
- [ ] 实现当前用户访问器，供业务 Service 使用。
- [ ] 实现 `/api/auth/user`。
- [ ] 测试无 token 返回 401。
- [ ] 测试 token 格式错误返回 401。
- [ ] 测试缓存未命中时调用 InnerServer。
- [ ] 测试缓存命中时不调用 InnerServer。

## 验收标准

- `/api/auth/user` 与原路径兼容。
- 需要鉴权的接口能拿到当前用户。
- Redis 缓存 TTL 明确。
- 401、业务异常、外部权限中心失败都有稳定响应。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
