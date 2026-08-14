# Day 08 - GitLab Client 与 Application 模块

## 今日目标

实现 GitLab Client 的基础能力，并完成 Application 模块。

## 今天学习的 .NET 点

- `IHttpClientFactory` typed client。
- Bearer token header。
- Options 注入多 registry 配置。
- 外部 API Mock 测试。

## 实现 Todo

- [ ] 实现 Git options：defaultRegistryKey、registries。
- [ ] 实现 GitLab Client：searchProject、getProjectById、getPipelineTriggerToken。
- [ ] 实现 ensurePipelineTriggerToken，包括 token 不可用时重建。
- [ ] 实现 Application create DTO、update DTO、query DTO。
- [ ] 实现 `/api/application/create`。
- [ ] create 时读取 GitLab 项目、保存 gitName/gitRepo/gitNamespaceId。
- [ ] create 时绑定 templates 和 owner。
- [ ] 实现 `/api/application/update`，更新主应用并同步子应用 Git 信息。
- [ ] 实现 detail、findAll、findGit、search、list、delete。
- [ ] 测试 Git 项目不存在。
- [ ] 测试 registryKey 默认值。
- [ ] 测试 trigger token 刷新。

## 验收标准

- Application 模块核心接口可用。
- GitLab Client 可以通过 Mock 验证成功和失败。
- 应用更新时子应用 Git 信息同步。
- 真实密钥不进入源码。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
