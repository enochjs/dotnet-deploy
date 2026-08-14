# Day 04 - Migration、种子数据和 Testcontainers

## 今日目标

让数据库和测试环境可重复创建。今天结束后，应能在干净 PostgreSQL/Redis 环境中跑起集成测试。

## 今天学习的 .NET 点

- EF Core Migration。
- `dotnet ef migrations add`、`dotnet ef database update`。
- Testcontainers for PostgreSQL/Redis。
- 测试生命周期：初始化、执行、清理。

## 实现 Todo

- [ ] 添加 EF Core CLI 工具配置。
- [ ] 生成第一版 migration。
- [ ] 人工检查 migration SQL，重点看表名、列名、uuid、自增、jsonb。
- [ ] 创建测试用种子数据：用户、应用、子应用、模板、需求、迭代。
- [ ] 在 IntegrationTests 中启动 PostgreSQL Testcontainer。
- [ ] 在 IntegrationTests 中启动 Redis Testcontainer。
- [ ] 测试启动时自动应用 migration。
- [ ] 写数据库连接测试。
- [ ] 写 Redis set/get 测试。
- [ ] 写健康检查集成测试。

## 验收标准

- 全新数据库可以通过 migration 创建表。
- 集成测试不依赖你本机已有数据库。
- 种子数据能支撑后续基础模块测试。
- `dotnet test` 能跑通数据库和 Redis 基础测试。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
