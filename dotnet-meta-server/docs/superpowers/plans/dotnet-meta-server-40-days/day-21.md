# Day 21 - Rancher/VPN Deploy Listener

## 今日目标

实现 Rancher 普通部署和 VPN 部署 listener，并写入 Deploy 发布记录。

## 今天学习的 .NET 点

- 外部 HTTP 调用失败处理。
- 业务副作用：状态更新 + 发布记录。
- 从 nested extra 中读取参数。
- 超时和重试的边界。

## 实现 Todo

- [ ] 实现 Rancher/VPN deploy options。
- [ ] 实现 Rancher deploy typed client。
- [ ] 实现普通 Rancher deploy listener create/success/failed/canceled。
- [ ] 实现 VPN deploy listener create/success/failed/canceled。
- [ ] 提取 image、env、deployKey、namespace、swimLane、platform。
- [ ] 实现 deployKey 泳道处理规则。
- [ ] 部署成功后写入 Deploy 表。
- [ ] 部署失败后更新 PipelineJob 为 failed。
- [ ] 测试部署成功、部署失败、容器不存在、缺少镜像、缺少 deployKey。

## 验收标准

- 普通和 VPN 部署 listener 都可通过 Mock 验证。
- Deploy 表能记录 appKey、env、version、pipelineId、swimLane、integrationReleaseVersion。
- 部署失败不会让 Pipeline 假成功。
- 日志能定位部署参数和失败原因。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
