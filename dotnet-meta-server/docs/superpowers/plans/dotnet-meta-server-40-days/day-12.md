# Day 12 - Iteration 模块

## 今日目标

实现迭代模块。迭代连接需求、应用、子应用、分支和后续 Pipeline。

## 今天学习的 .NET 点

- 多对多关系更新。
- 聚合详情 DTO。
- 事务辅助方法。
- 查询条件组合。

## 实现 Todo

- [ ] 实现 Iteration DTO：create、update、query、detail。
- [ ] 实现 `/api/iteration/create`，关联 requirementIds。
- [ ] 实现 `/api/iteration/relation/list`，按当前用户相关需求和创建人过滤。
- [ ] 实现 `/api/iteration/detail/:id`，返回 iteration、appInfo、templates。
- [ ] 实现 `/api/iteration/detailByBranch`。
- [ ] 实现 update，重建需求关联。
- [ ] 实现 finish、delete、findAll、list。
- [ ] 实现 `CreateForIntegrationRelease` 内部方法，为后续集成发布复用。
- [ ] 测试需求关联、分支查询、应用不存在、集成发布迭代过滤。

## 验收标准

- 迭代 CRUD 全部兼容原接口。
- detail 能返回子应用模板。
- 普通迭代和集成发布派生迭代能区分。
- 后续 Pipeline 能通过 iteration 找到 subApplication。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
