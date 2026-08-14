# Day 13 - 基础模块前端兼容回归

## 今日目标

把 Auth、User、Application、SubApplication、Template、Requirement、Iteration 用前端请求形状完整回放一次。

## 今天学习的 .NET 点

- WebApplicationFactory 集成测试。
- HTTP request/response 快照。
- JSON 字段兼容校验。
- CORS 和前端代理。

## 实现 Todo

- [ ] 准备基础模块 HTTP 回放文件或集成测试。
- [ ] 回放 `/api/auth/user`。
- [ ] 回放 `/api/user/*`。
- [ ] 回放 `/api/application/*` 和 `/api/application/sub/*`。
- [ ] 回放 `/api/pipeline/tpl/*`。
- [ ] 回放 `/api/demand/*`。
- [ ] 回放 `/api/iteration/*`。
- [ ] 检查字段名、状态值、分页结构、错误结构、日期格式。
- [ ] 修复 P0/P1 兼容问题。
- [ ] 形成基础模块兼容报告。

## 验收标准

- 基础模块前端请求能跑通。
- 所有不兼容项都有修复或明确记录。
- `dotnet test` 覆盖基础模块主链路。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
