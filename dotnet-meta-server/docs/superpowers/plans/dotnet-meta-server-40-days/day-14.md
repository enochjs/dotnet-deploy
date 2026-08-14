# Day 14 - Pipeline 状态机骨架

## 今日目标

搭建 Pipeline 状态机的队列和事件派发骨架，不急着实现所有 listener。

## 今天学习的 .NET 点

- BackgroundService、Channel、Hangfire 的取舍。
- 内部事件派发。
- 命令对象和 Handler。
- 幂等基础。

## 实现 Todo

- [ ] 定义 Pipeline processor 事件枚举或常量。
- [ ] 定义 listener name 常量，保持原字符串兼容。
- [ ] 定义 job payload：CreatePipeline、StageCreate、JobForward、JobStateChange。
- [ ] 实现队列抽象：Enqueue、Consume、Retry、Log failure。
- [ ] 实现事件派发抽象：listenerName + status -> handler。
- [ ] 实现日志上下文：pipelineId、tplId、jobKey、stageSeq。
- [ ] 写一个 Fake handler 验证事件能从队列到 handler。
- [ ] 设计 Redis stage cache key：pipelineId + stageSeq。

## 验收标准

- 队列和事件派发骨架能跑通。
- handler 注册方式清晰。
- 失败有日志，不会静默吞掉。
- 后续 listener 可以按统一模式接入。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
