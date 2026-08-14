# Day 07 - Pipeline Template 模块

## 今日目标

实现流水线模板模块。模板是 Pipeline 状态机的基础，必须在进入 Pipeline 前稳定。

## 今天学习的 .NET 点

- EF Core 保存父子集合。
- `Include`、`ThenInclude` 和排序。
- 软删除。
- 嵌套 DTO 校验。

## 实现 Todo

- [ ] 实现模板、阶段、任务 DTO。
- [ ] 实现 `/api/pipeline/tpl/create`。
- [ ] create 时 trim `templateKey` 并检查唯一性。
- [ ] create 时自动维护 stage `seq` 和 job `stageSeq`。
- [ ] 实现 `/api/pipeline/tpl/update`。
- [ ] 实现 `/api/pipeline/tpl/delete/:id`，使用软删除状态。
- [ ] 实现 `/api/pipeline/tpl/detail/:id`，阶段按 seq 排序。
- [ ] 实现 `/api/pipeline/tpl/list` 和 `/search`。
- [ ] 测试 templateKey 为空。
- [ ] 测试 templateKey 重复。
- [ ] 测试 stage/job 顺序。
- [ ] 测试软删除后查询过滤。

## 验收标准

- 模板 CRUD 接口兼容原路径。
- 嵌套 stage/job 保存正确。
- 软删除语义正确。
- 后续 Pipeline 可通过模板获取完整 stage/job。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
