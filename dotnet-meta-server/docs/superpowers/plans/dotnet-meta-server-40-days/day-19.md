# Day 19 - Pipeline 查询聚合与状态机测试

## 今日目标

实现 Pipeline active/history 查询，并补齐状态机主链路测试。

## 今天学习的 .NET 点

- 多表聚合 DTO。
- 批量查询减少 N+1。
- 状态机测试。
- 测试异步队列最终一致性。

## 实现 Todo

- [ ] 实现 `/api/pipeline/iteration/active`。
- [ ] 实现 `/api/pipeline/iteration/history/list`。
- [ ] 按 iterationId 查询 pipeline。
- [ ] 批量查询 pipeline jobs。
- [ ] 批量查询 templates。
- [ ] 合并 template stage/job 和 runtime job extra。
- [ ] 测试 active 状态范围：CREATE、IN_PROGRESS、FAILED。
- [ ] 测试 history 状态范围：SUCCESS、CANCELED。
- [ ] 写两阶段三任务状态机测试。
- [ ] 测试任意 job failed/canceled 时 pipeline 状态变化。

## 验收标准

- 前端能拿到完整 stage/job 展示结构。
- active/history 分类正确。
- 状态机成功、失败、取消路径都有测试。
- 查询没有明显 N+1 问题。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
