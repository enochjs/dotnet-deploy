# Day 16 - Pipeline 主监听器与手动卡点

## 今日目标

实现 PIPELINE 主监听器、Manual listener，以及 create/next/stop/retry 接口。

## 今天学习的 .NET 点

- Handler 生命周期。
- Controller 只发命令，不直接跑长流程。
- 状态检查和早返回。
- 取消远程任务的补偿动作。

## 实现 Todo

- [ ] 实现 `/api/pipeline/create`，只入队创建事件。
- [ ] 实现 PIPELINE create handler：创建 Pipeline 并派发第一个 stage。
- [ ] 实现 PIPELINE success/failed/canceled handler。
- [ ] 实现 stage dispatch：找下一个 stage，执行 stage jobs。
- [ ] 实现 Manual listener create/success/canceled。
- [ ] 实现 `/api/pipeline/stage/next`。
- [ ] 实现 `/api/pipeline/stage/stop`。
- [ ] 实现 `/api/pipeline/retry/job`。
- [ ] Build 重试时取消旧 GitLab pipeline 的接口先接 mock。
- [ ] 测试 stageSeq 不匹配、pipeline 不存在、手动通过、停止、重试。

## 验收标准

- 简单模板能从 create 推进到 manual 卡点。
- 手动 next 能继续推进。
- stop 能取消 pipeline。
- retry 只允许当前 stage job。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
