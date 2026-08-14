# Day 10 - Requirement 基础功能

## 今日目标

实现需求模块基础功能：创建、详情、列表、关注、人员维护、更新、完成和删除。

## 今天学习的 .NET 点

- 多对多关系查询和保存。
- 集合去重。
- 动态查询条件。
- Patch/Put 的实现差异。

## 实现 Todo

- [ ] 实现 Requirement DTO：create、update、query、detail、toggleFollow。
- [ ] 实现 `/api/demand/create`，创建人自动加入 followers。
- [ ] 实现 `/api/demand/relation/list`。
- [ ] relation/list 按创建人、开发人、关注人过滤当前用户相关需求。
- [ ] 实现 `/api/demand/relation/search`。
- [ ] 实现 `/api/demand/detail/:id`，聚合关联迭代摘要。
- [ ] 实现 `/api/demand/toggle-follow/:id`。
- [ ] 实现 update、delete、finish。
- [ ] 实现 add/users 和 delete/user。
- [ ] 实现 `/api/demand/list`，支持 status 多格式查询。
- [ ] 测试创建人自动关注、重复用户去重、取消关注、软删除过滤、status 数组查询。

## 验收标准

- Requirement 基础接口全部可用。
- users/followers 多对多关系正确。
- 软删除和完成状态正确。
- 需求详情能展示关联迭代基础信息。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
