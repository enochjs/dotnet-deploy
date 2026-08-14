# Day 17 - GitLab MR Listener

## 今日目标

实现 Merge Request listener，把 GitLab MR 流程接入 PipelineJob。

## 今天学习的 .NET 点

- 复杂 handler 拆小私有方法。
- 轮询、等待和超时。
- 外部 API 错误转业务状态。
- 单元测试 mock 多步外部调用。

## 实现 Todo

- [ ] 补齐 GitLab Client：branch info、compare、create MR、get MR、accept MR。
- [ ] 实现 unitKey 生成和解析。
- [ ] 实现 MR listener create：获取分支、检查是否需要合并、查找/创建 MR。
- [ ] 支持已有 open MR 复用。
- [ ] 支持自动合并和手动合并模式。
- [ ] 保存 PipelineJob，写入 unitKey 和 MR extra。
- [ ] 实现 MR listener success/failed。
- [ ] 测试无需合并、创建 MR 成功、创建 MR 失败、自动合并失败、webhook 成功。

## 验收标准

- MR job 能被创建并记录 unitKey。
- MR webhook 能找到 active job。
- 不需要合并时能直接成功。
- 失败原因能写入 job extra 或日志。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
