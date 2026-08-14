# Dotnet Meta Server 40 个工作日迁移学习计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 用两个月、约 40 个自然工作日完成 `meta-server` 的 .NET 迁移。你已经非常熟悉原 `meta-server` 代码和业务，所以计划省略“熟悉功能”环节，重点放在 C#/.NET 学习、工程落地、功能实现、测试、联调和上线准备。

**Architecture:** 采用 ASP.NET Core Web API + EF Core + PostgreSQL + Redis + 后台队列 + SignalR 的结构。每天围绕一个可交付模块安排：先学当天必须用到的 .NET 概念，再实现对应功能，最后补测试和验收。

**Tech Stack:** C#、.NET 8/9、ASP.NET Core Web API、EF Core、PostgreSQL、Redis、SignalR、HttpClientFactory、Serilog、FluentValidation、xUnit、Testcontainers。

## 全局约束

- 不再安排 meta-server 业务熟悉环节；默认你知道原模块、字段、状态和业务流程。
- 每天的 C#/.NET 学习必须服务于当天实现，不做泛泛教程。
- 40 天按两个月工作日规划，每天都要有可检查产物。
- API 路径、请求字段、响应结构、表名、字段名、枚举值优先兼容原系统。
- 真实 token/secret 不允许进入源码，必须使用环境变量、User Secrets 或部署 Secret。
- 每个模块至少有单元测试；Pipeline、Webhook、批量部署、钉钉建需求、监控上报必须有集成测试或可重复回放脚本。
- 每天结束时记录三件事：今天学到的 .NET 点、今天完成的功能、明天风险。

## 每日文档目录

- [Day 01 - 解决方案骨架与 C# 快速上手](dotnet-meta-server-40-days/day-01.md)
- [Day 02 - 配置、日志、异常和统一响应](dotnet-meta-server-40-days/day-02.md)
- [Day 03 - EF Core 实体映射和 PostgreSQL 表结构](dotnet-meta-server-40-days/day-03.md)
- [Day 04 - Migration、种子数据和 Testcontainers](dotnet-meta-server-40-days/day-04.md)
- [Day 05 - 鉴权、当前用户和 InnerServer](dotnet-meta-server-40-days/day-05.md)
- [Day 06 - User 模块与 LINQ CRUD](dotnet-meta-server-40-days/day-06.md)
- [Day 07 - Pipeline Template 模块](dotnet-meta-server-40-days/day-07.md)
- [Day 08 - GitLab Client 与 Application 模块](dotnet-meta-server-40-days/day-08.md)
- [Day 09 - SubApplication、变量和泳道](dotnet-meta-server-40-days/day-09.md)
- [Day 10 - Requirement 基础功能](dotnet-meta-server-40-days/day-10.md)
- [Day 11 - DingTalk Client 与钉钉建需求](dotnet-meta-server-40-days/day-11.md)
- [Day 12 - Iteration 模块](dotnet-meta-server-40-days/day-12.md)
- [Day 13 - 基础模块前端兼容回归](dotnet-meta-server-40-days/day-13.md)
- [Day 14 - Pipeline 状态机骨架](dotnet-meta-server-40-days/day-14.md)
- [Day 15 - PipelineService、PipelineJobService 和 Redis Stage Cache](dotnet-meta-server-40-days/day-15.md)
- [Day 16 - Pipeline 主监听器与手动卡点](dotnet-meta-server-40-days/day-16.md)
- [Day 17 - GitLab MR Listener](dotnet-meta-server-40-days/day-17.md)
- [Day 18 - Build Listener 与 GitLab Pipeline Webhook](dotnet-meta-server-40-days/day-18.md)
- [Day 19 - Pipeline 查询聚合与状态机测试](dotnet-meta-server-40-days/day-19.md)
- [Day 20 - SignalR 实时通知](dotnet-meta-server-40-days/day-20.md)
- [Day 21 - Rancher/VPN Deploy Listener](dotnet-meta-server-40-days/day-21.md)
- [Day 22 - Static Deploy 与 OSS](dotnet-meta-server-40-days/day-22.md)
- [Day 23 - DingTalk Approve Listener](dotnet-meta-server-40-days/day-23.md)
- [Day 24 - Deploy 查询与部署链路回归](dotnet-meta-server-40-days/day-24.md)
- [Day 25 - IntegrationRelease 创建、详情和矩阵](dotnet-meta-server-40-days/day-25.md)
- [Day 26 - IntegrationRelease 批量部署和流水线聚合](dotnet-meta-server-40-days/day-26.md)
- [Day 27 - bugfix-from-requirement](dotnet-meta-server-40-days/day-27.md)
- [Day 28 - Requirement 批量部署和跨模块复用](dotnet-meta-server-40-days/day-28.md)
- [Day 29 - Monitor 模块](dotnet-meta-server-40-days/day-29.md)
- [Day 30 - Page fallback、Swagger 和前端代理联调](dotnet-meta-server-40-days/day-30.md)
- [Day 31 - API 兼容性修正日](dotnet-meta-server-40-days/day-31.md)
- [Day 32 - 数据迁移和历史数据兼容](dotnet-meta-server-40-days/day-32.md)
- [Day 33 - Pipeline 全链路压测和幂等](dotnet-meta-server-40-days/day-33.md)
- [Day 34 - 外部系统 Mock 与真实环境联调](dotnet-meta-server-40-days/day-34.md)
- [Day 35 - 端到端验收脚本](dotnet-meta-server-40-days/day-35.md)
- [Day 36 - 测试覆盖补齐和代码质量](dotnet-meta-server-40-days/day-36.md)
- [Day 37 - Docker、部署配置和健康检查](dotnet-meta-server-40-days/day-37.md)
- [Day 38 - 性能、日志和可观测性](dotnet-meta-server-40-days/day-38.md)
- [Day 39 - 预发布演练和回滚方案](dotnet-meta-server-40-days/day-39.md)
- [Day 40 - 最终验收、交接和学习总结](dotnet-meta-server-40-days/day-40.md)

## 推荐日节奏

- 09:30-10:15：学习当天 .NET 概念，只学马上会用到的部分。
- 10:15-12:00：写测试或接口契约，明确当天验收口径。
- 13:30-17:30：实现功能，小步提交。
- 17:30-18:30：跑测试、修兼容问题、记录学习笔记和风险。

## 阶段划分

- Day 01-04：工程骨架、配置、数据库、测试底座。
- Day 05-13：基础业务模块和前端兼容。
- Day 14-24：Pipeline、Webhook、Deploy、SignalR。
- Day 25-30：集成发布、bugfix、需求批量部署、监控。
- Day 31-40：兼容修正、数据迁移、压测、联调、部署和最终验收。

## 当前计划与旧计划的差异

- 从 60 天缩短为两个月约 40 个工作日。
- 省略熟悉 `meta-server` 功能的环节。
- 不再写并行分工，改为单人自然工作日节奏。
- 当前执行入口为 `dotnet-meta-server-40-days`；旧 `dotnet-meta-server-migration-days` 目录只保留为历史草稿。
