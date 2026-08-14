# Day 01 - 解决方案骨架与 C# 快速上手

## 今日目标

创建 .NET 解决方案骨架，并把 C# 基础语法和项目结构跑通。今天不做业务模块，重点是把后续 39 天的工程地基搭好。

## 今天学习的 .NET 点

- `.sln` 和 `.csproj` 的关系。
- `Program.cs`、依赖注入容器、ASP.NET Core 请求管线。
- C# namespace、class、record、interface、async/await 的最小用法。
- `dotnet build`、`dotnet run`、`dotnet test` 的区别。

## 实现 Todo

- [ ] 创建解决方案 `DotnetMetaServer.sln`。
- [ ] 创建项目：`Api`、`Application`、`Domain`、`Infrastructure`、`UnitTests`、`IntegrationTests`。
- [ ] 建立引用方向：`Api -> Application -> Domain`，`Infrastructure -> Application/Domain`，测试项目引用被测项目。
- [ ] 在 `Api` 中启用 Swagger。
- [ ] 添加 `/health` 接口或健康检查。
- [ ] 建立 `Directory.Build.props`，统一 nullable、implicit using、语言版本。
- [ ] 写一条最小 smoke test：应用能创建测试服务器。
- [ ] 记录今天遇到的 C# 语法：namespace、using、public、async。

## 验收标准

- `dotnet build` 通过。
- `dotnet test` 通过。
- Swagger 能打开，`/health` 能返回成功。
- 你能说明每个项目目录负责什么。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
