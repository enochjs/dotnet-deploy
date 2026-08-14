# Day 20 - SignalR 实时通知

## 今日目标

实现 Pipeline 实时通知，让前端能在 Pipeline 或 PipelineJob 更新时刷新。

## 今天学习的 .NET 点

- SignalR Hub。
- WebSocket 鉴权。
- Group 分组推送。
- 连接断开和重连。

## 实现 Todo

- [ ] 设计 PipelineHub。
- [ ] 设计连接鉴权，复用现有 Bearer token 或 query token。
- [ ] 按 iterationId 建 group。
- [ ] 按 pipelineId 建 group。
- [ ] 实现 notifyIteration。
- [ ] 实现 notifyPipelineId。
- [ ] 在 PipelineService 状态更新后触发 notifyIteration。
- [ ] 在 PipelineJobService create/update 后触发 notifyPipelineId。
- [ ] 写 SignalR 集成测试或最小手工测试页面。
- [ ] 测试未授权连接、正常连接、状态更新推送、断开清理。

## 验收标准

- Pipeline 状态变更能推送给订阅方。
- PipelineJob 状态变更能推送给订阅方。
- 未授权连接不能订阅。
- 前端无需高频轮询即可更新状态。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
