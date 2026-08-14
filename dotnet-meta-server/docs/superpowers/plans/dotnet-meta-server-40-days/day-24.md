# Day 24 - Deploy 查询与部署链路回归

## 今日目标

实现 Deploy 查询接口，并把 MR -> Build -> Manual -> Deploy 跑成最小闭环。

## 今天学习的 .NET 点

- 查询过滤和分页。
- 集成测试组织。
- Smoke test。
- 失败定位方法。

## 实现 Todo

- [ ] 实现 `/api/deploy/search`。
- [ ] 实现 `/api/deploy/list`。
- [ ] 支持 appKey、env、version、swimLane、integrationReleaseVersion 过滤。
- [ ] 按 createTime 倒序。
- [ ] 准备最小模板：MR -> Build -> Manual -> Deploy。
- [ ] 创建 Pipeline。
- [ ] 模拟 MR 成功。
- [ ] 模拟 Build 成功。
- [ ] 调用 Manual next。
- [ ] 模拟 Deploy 成功。
- [ ] 查询 Deploy 记录。
- [ ] 修复部署链路 P0/P1 问题。

## 验收标准

- Deploy 查询接口兼容原路径。
- 最小部署 Pipeline 能跑到成功。
- Deploy 记录可查。
- 每一步状态变化有日志。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
