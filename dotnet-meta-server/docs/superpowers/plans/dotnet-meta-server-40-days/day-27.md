# Day 27 - bugfix-from-requirement

## 今日目标

实现需求详情一键合入 bugfix 的复杂用例。

## 今天学习的 .NET 点

- 复杂 use case 编排。
- 日期版本生成。
- 分支名清洗。
- 局部失败返回。

## 实现 Todo

- [ ] 实现 BugfixFromRequirement DTO。
- [ ] 生成版本：`bug_fix_YYYY-MM-DD`。
- [ ] 查找或创建当天 bugfix release。
- [ ] 查找 `template_key=bugfix_merge` 模板。
- [ ] 校验需求和关联迭代。
- [ ] 检查源分支存在。
- [ ] 查找或创建目标 release_app 和目标 iteration。
- [ ] 检查或创建目标分支。
- [ ] 创建 bugfix merge Pipeline queue payload。
- [ ] extra 写入 sourceIteration、targetIteration、mergeRequestSourceBranch、mergeRequestTargetBranch。
- [ ] 返回 success/failed。
- [ ] 测试需求不存在、无迭代、模板缺失、源分支不存在、目标分支创建失败、成功创建 pipeline。

## 验收标准

- bugfix release 可复用当天版本。
- 目标迭代可复用或创建。
- 每个源迭代失败都有 reason。
- bugfix pipeline extra 完整。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
