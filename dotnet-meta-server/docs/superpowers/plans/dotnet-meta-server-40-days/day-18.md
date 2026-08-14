# Day 18 - Build Listener 与 GitLab Pipeline Webhook

## 今日目标

实现 Build listener 和 GitLab webhook 中 pipeline 回调处理。

## 今天学习的 .NET 点

- multipart/form-data 或 form url encoded 请求。
- 动态 JSON 解析 webhook。
- 外部回调快速返回。
- 状态映射。

## 实现 Todo

- [ ] 补齐 GitLab Client：triggerPipeline、cancelPipeline、getJobLog。
- [ ] 实现 Build listener create。
- [ ] 组装 CI 变量：EXECUTE_TYPE、BUILD_ENV、BUILD_BRANCH、IMAGE_VERSION、RUN_TYPE、APP_KEY、SWIM_LANE 等。
- [ ] trigger 成功后保存 PipelineJob 和 unitKey。
- [ ] trigger 失败时 job failed。
- [ ] 实现 `/api/pipeline/git/callback`。
- [ ] 实现 `/api/pipeline/git/:registryKey/callback`。
- [ ] 处理 object_kind=pipeline：success、failed、canceled。
- [ ] legacy registry fallback 兼容。
- [ ] 测试 trigger 成功/失败、webhook 找不到 job、重复 webhook。

## 验收标准

- Build job 能触发 GitLab pipeline。
- GitLab pipeline webhook 能推动 job 状态。
- 找不到 job 时不影响 webhook 返回。
- 重复 webhook 不导致状态错乱。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
