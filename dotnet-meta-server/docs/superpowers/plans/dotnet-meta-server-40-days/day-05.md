# Day 05 - 本地登录、JWT 鉴权和当前用户

## 今日目标

实现企业项目里更常见的本地账号密码登录和标准 JWT Bearer 鉴权。这个模块小，但会贯穿所有需要登录的 API。

## 今天学习的 .NET 点

- ASP.NET Core Authentication/Authorization。
- `Microsoft.AspNetCore.Authentication.JwtBearer` 标准鉴权流程。
- `PasswordHasher<TUser>` 密码哈希。
- 从 JWT claims 读取当前用户。
- 自定义当前用户上下文。
- 认证配置、签名密钥、Issuer、Audience、过期时间。

## 实现 Todo

- [ ] 给 `User` 增加 `PasswordHash` 字段。
- [ ] 实现 `JwtOptions` 配置和启动校验。
- [ ] 实现密码哈希服务。
- [ ] 实现 JWT 签发服务。
- [ ] 实现本地登录服务：使用 `userId` 或 `mobile` + password 登录。
- [ ] 注册标准 JWT Bearer 鉴权。
- [ ] 实现当前用户访问器，供业务 Service 使用。
- [ ] 实现 `/api/auth/login`。
- [ ] 实现 `/api/auth/user`。
- [ ] 测试密码错误返回稳定业务错误。
- [ ] 测试无 token 返回 401。
- [ ] 测试 token 格式错误返回 401。
- [ ] 测试登录成功能拿到 accessToken。
- [ ] 测试带 accessToken 能访问当前用户。
- [ ] 测试过期 token 返回 401。

## 验收标准

- `/api/auth/login` 使用本地数据库用户和密码登录。
- `/api/auth/user` 与原路径兼容。
- 需要鉴权的接口能通过 `[Authorize]` 拿到当前用户。
- 密码不以明文保存。
- JWT 的 Issuer、Audience、SigningKey、过期时间都有配置。
- 401、业务异常、token 过期都有稳定响应。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
