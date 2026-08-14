# Day 25 - IntegrationRelease 创建、详情和矩阵

## 今日目标

实现集成发布创建、分页、详情和子应用矩阵。

## 今天学习的 .NET 点

- 数据库事务。
- 多表聚合。
- 批量外部 API 调用的并发控制。
- Git compare 结果建模。

## 实现 Todo

- [ ] 实现 IntegrationRelease DTO：create、query、detail、subAppMatrix。
- [ ] 实现 `/api/integration-release/create`。
- [ ] 校验 version 唯一。
- [ ] 校验 subApplicationIds 去重和存在性。
- [ ] 用事务创建 release、branch、iteration、release_app。
- [ ] 创建分支失败时整体回滚。
- [ ] 实现 `/api/integration-release/page`。
- [ ] 实现 `/api/integration-release/detail/:id`。
- [ ] 实现 `/api/integration-release/sub-app-matrix`。
- [ ] 矩阵按泳道检查 main/pre 分支和 pre 是否 ahead main。
- [ ] 测试重复版本、重复子应用、分支创建失败、事务回滚、矩阵检查失败。

## 验收标准

- 集成发布创建具备事务保护。
- release_app 和派生 iteration 关系正确。
- detail 能返回 apps、branch、templates。
- sub-app-matrix 能通过 Mock GitLab 验证。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
