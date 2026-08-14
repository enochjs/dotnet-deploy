# Day 06 - User 模块与 LINQ CRUD

## 今日目标

实现用户模块，练熟 ASP.NET Core Controller、Service、EF Core LINQ 查询和分页。

## 今天学习的 .NET 点

- LINQ：`Where`、`AnyAsync`、`FirstOrDefaultAsync`、`Skip`、`Take`。
- DTO 与 Entity 映射。
- FluentValidation 基础。
- 业务异常和唯一性校验。

## 实现 Todo

- [ ] 实现用户创建 DTO、更新 DTO、查询 DTO、返回 DTO。
- [ ] 实现 `/api/user/create`，包含手机号唯一校验。
- [ ] 实现 `/api/user/update/:id`，包含 manager 信息更新。
- [ ] 实现 `/api/user/detail/:id`。
- [ ] 实现 `/api/user/search`，本地查不到时调用 InnerServer 并同步。
- [ ] 实现 `/api/user/list` 分页。
- [ ] 测试手机号重复。
- [ ] 测试用户不存在。
- [ ] 测试按 name、realName、userId 搜索。
- [ ] 测试分页排序。

## 验收标准

- User 模块所有原接口都有 .NET 等价实现。
- 分页结构兼容。
- 用户同步路径可 Mock。
- LINQ 查询都有测试覆盖核心条件。

## 晚上复盘

- 今天学会的 C#/.NET 概念：
- 今天完成的工程产物：
- 明天风险：
