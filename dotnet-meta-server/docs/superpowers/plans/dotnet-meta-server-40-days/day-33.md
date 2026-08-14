# Day 33 - Pipeline 全链路压测和幂等

## 今日目标

验证 Pipeline 在重复 webhook、并发入队、Redis key 失效、外部系统失败下的稳定性。

## 今天学习的 .NET 点

- 幂等设计。
- 并发测试。
- Redis TTL。
- 后台队列故障恢复。

## 实现 Todo

- [ ] 并发创建多个 Pipeline。
- [ ] 重复发送 MR webhook。
- [ ] 重复发送 Build webhook。
- [ ] 重复点击 stage next。
- [ ] 重复点击 retry job。
- [ ] 模拟 Redis stage cache 过期。
- [ ] 模拟 GitLab 网络超时。
- [ ] 模拟 Rancher deploy 失败。
- [ ] 检查最终 Pipeline/PipelineJob 状态。
- [ ] 补充幂等保护或日志。

## 验收标准

- 重复 webhook 不导致重复推进 stage。
- 已结束 Pipeline 不再执行新 job。
- Redis 异常有可诊断日志。
- 外部系统失败能落到明确 job 状态。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
