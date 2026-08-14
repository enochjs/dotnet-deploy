# Day 23 - DingTalk Approve Listener

## 今日目标

实现钉钉审批 listener，把发布审批纳入 Pipeline 状态机。

## 今天学习的 .NET 点

- 长流程外部状态建模。
- 审批实例 ID 存储。
- 外部回调/下行消息处理。
- Handler 可测试设计。

## 实现 Todo

- [ ] 实现 DingTalk createApproveInstance。
- [ ] 实现 approve listener create。
- [ ] 组装审批表单字段：应用、环境、版本、内容。
- [ ] 处理审批人列表和 AND/OR。
- [ ] 创建审批成功后保存 PipelineJob extra。
- [ ] 创建审批失败时 job failed。
- [ ] 实现审批成功事件处理。
- [ ] 实现审批失败事件处理。
- [ ] 如原系统需要下行消息，接入 PipelineDdService 等价服务。
- [ ] 测试无审批人、审批创建失败、审批通过、审批拒绝。

## 验收标准

- 审批 listener 能创建并记录审批实例。
- 审批结果能推动 job 状态。
- 审批失败不阻塞日志排查。
- DingTalk Client 可 Mock。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
