# Day 22 - Static Deploy 与 OSS

## 今日目标

实现静态发布 listener 和 OSS version 文件更新。

## 今天学习的 .NET 点

- 文件流和临时文件。
- JSON 序列化。
- finally 清理资源。
- 云存储 Client 抽象。

## 实现 Todo

- [ ] 实现 OSS options。
- [ ] 实现 OSS client 抽象和 Ali OSS 适配层。
- [ ] 实现 `UpdateVersion`：生成 version、forceUpdate、timestamp JSON。
- [ ] 上传路径兼容：`appKey/env/swimLane/check/version.json`。
- [ ] 实现 Static deploy listener create/success/failed。
- [ ] 部署成功后写 Deploy 表。
- [ ] 部署失败后写 job failed。
- [ ] 测试 OSS 成功、OSS 失败、临时文件清理、forceUpdate 默认值、路径正确。

## 验收标准

- OSS 路径和文件内容兼容原逻辑。
- 临时文件不会残留。
- Static deploy 能推动 PipelineJob 状态。
- OSS 失败有明确错误和日志。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
