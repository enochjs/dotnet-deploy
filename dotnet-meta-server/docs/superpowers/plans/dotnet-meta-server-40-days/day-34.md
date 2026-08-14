# Day 34 - 外部系统 Mock 与真实环境联调

## 今日目标

把 GitLab、DingTalk、Rancher/VPN、OSS、InnerServer 的 Mock 测试和真实测试环境联调都跑一遍。

## 今天学习的 .NET 点

- HttpClient mock。
- 测试环境配置隔离。
- 超时、重试、失败降级。
- Secret 注入。

## 实现 Todo

- [ ] 跑 GitLab Mock 测试全量。
- [ ] 跑 DingTalk Mock 测试全量。
- [ ] 跑 InnerServer Mock 测试全量。
- [ ] 跑 Rancher/VPN Mock 测试全量。
- [ ] 跑 OSS Mock 测试全量。
- [ ] 配置测试环境真实 GitLab token。
- [ ] 配置测试环境 DingTalk 应用。
- [ ] 配置测试环境 OSS bucket。
- [ ] 执行最小真实联调：查项目、建分支或查分支、发送测试机器人消息、上传测试 version 文件。
- [ ] 记录真实联调不可用项和替代验证方式。

## 验收标准

- 外部 Client 都有 Mock 测试。
- 真实环境至少完成最小联调。
- 所有 secret 都来自环境变量或安全配置。
- 不可联调项有明确阻塞原因。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
