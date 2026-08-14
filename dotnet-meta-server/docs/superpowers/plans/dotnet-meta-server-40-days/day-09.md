# Day 09 - SubApplication、变量和泳道

## 今日目标

实现子应用模块，包括批量更新、模板绑定、变量保存和泳道复制。

## 今天学习的 .NET 点

- EF Core 更新集合。
- JSON 字段映射到 Dictionary/List。
- 字符串替换和正则。
- 处理可空 JSON 的防御式写法。

## 实现 Todo

- [ ] 实现 SubApplication DTO。
- [ ] 实现 `/api/application/sub/findAll`、detail、findGit、search、list、delete。
- [ ] 实现 `/api/application/sub/update`，支持新增和更新多个子应用。
- [ ] 子应用继承主应用 Git 信息和 triggerToken。
- [ ] 实现子应用模板绑定。
- [ ] 实现 `/api/application/sub/variables`。
- [ ] variables 按 `envKey-swimLaneKey` 保存。
- [ ] 实现 `/api/application/sub/swimLine`。
- [ ] 从 B 泳道复制变量到目标泳道。
- [ ] 特殊处理 `DEPLOY_NAMESPACE` 末尾泳道替换。
- [ ] 测试新增、更新、变量保存、泳道复制、空 variables。

## 验收标准

- 子应用批量更新不会误删已有数据。
- variables JSON 结构兼容原系统。
- 泳道复制规则正确。
- 后续 Pipeline 能通过 appKey 找到子应用和模板。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
