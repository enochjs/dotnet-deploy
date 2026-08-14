# Day 26 - IntegrationRelease 批量部署和流水线聚合

## 今日目标

实现集成发布维度批量部署，以及 active/history 流水线聚合。

## 今天学习的 .NET 点

- 批量操作部分成功。
- 单项异常隔离。
- 多来源数据聚合排序。
- DTO 组合。

## 实现 Todo

- [ ] 实现 `/api/integration-release/:id/batch-deploy`。
- [ ] 按 appKeys 查 release_app。
- [ ] 为每个 appKey 创建 Pipeline queue payload。
- [ ] 返回 success 和 failed 明细。
- [ ] extra 写入 integrationReleaseVersion 和 integrationReleaseId。
- [ ] 实现 `/api/integration-release/detail/:id/pipelines`。
- [ ] 聚合每个 release_app iteration 的 active/history。
- [ ] 按 createTime 倒序。
- [ ] 测试 release 不存在、appKey 不存在、部分成功、全部失败、active/history 排序。

## 验收标准

- 批量部署不会因单个 app 失败中断全部。
- 返回能明确每个 appKey 成功或失败原因。
- 集成发布详情能展示 active/history pipelines。
- Pipeline extra 包含集成发布信息。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
