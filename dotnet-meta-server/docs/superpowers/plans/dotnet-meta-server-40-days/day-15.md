# Day 15 - PipelineService、PipelineJobService 和 Redis Stage Cache

## 今日目标

实现 Pipeline 和 PipelineJob 的基础服务，以及 stage 内 job 完成检查。

## 今天学习的 .NET 点

- UUID 主键实体保存。
- Redis JSON 存取。
- 并发下的状态更新。
- Service 间依赖拆分。

## 实现 Todo

- [ ] 实现 PipelineService.CreatePipeline。
- [ ] CreatePipeline 从 iteration 找 application/subApplication。
- [ ] CreatePipeline 注入 integrationRelease extra。
- [ ] 实现 PipelineService success/fail/cancel/modify。
- [ ] 实现 PipelineJobService create/update/findActiveByUnitKey。
- [ ] 实现 getPipelineByIterationId 的基础查询。
- [ ] 实现 Redis stage cache：初始化、标记 job 完成、判断 stage 完成、删除。
- [ ] 测试迭代不存在、应用不存在、创建成功。
- [ ] 测试单 stage 多 job 完成判断。
- [ ] 测试 pipeline 已结束后不再推进。

## 验收标准

- Pipeline 和 PipelineJob 可创建和更新。
- Redis stage cache 能判断 stage 是否完成。
- active job 查询可被 webhook 使用。
- 核心状态更新有测试。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
