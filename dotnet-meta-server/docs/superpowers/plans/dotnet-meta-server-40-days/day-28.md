# Day 28 - Requirement 批量部署和跨模块复用

## 今日目标

实现需求维度批量部署和需求流水线聚合，并复用已经完成的 Pipeline 能力。

## 今天学习的 .NET 点

- 多对多关系筛选。
- 复用 Application Service 用例。
- 循环内异常隔离。
- 响应 DTO 复用和扩展。

## 实现 Todo

- [ ] 实现 `/api/demand/:id/batch-deploy`。
- [ ] 校验需求存在并加载 iterations。
- [ ] 校验 iterationIds 属于当前需求。
- [ ] 为每个 iteration 创建 Pipeline queue payload。
- [ ] extra 写入 requirementId、requirementName。
- [ ] 返回 success/failed。
- [ ] 实现 `/api/demand/detail/:id/pipelines`。
- [ ] 聚合需求关联迭代的 active/history pipelines。
- [ ] 过滤软删除迭代。
- [ ] 测试需求不存在、迭代不属于需求、部分失败、软删除迭代跳过。

## 验收标准

- 需求批量部署可复用 Pipeline。
- 返回成功/失败明细。
- 需求详情能展示关联流水线。
- extra 能追溯 requirement。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
