# 01. 后端功能开发流程

这份文档按“开发一个功能”的顺序组织。你可以把它当成新功能的操作手册：先分析需求，再决定接口，再补 DTO、校验、Service、Repository、Controller、测试。

## 0. 开发前先判断功能类型

开始写代码前，先判断你要做的是哪一种功能。不同功能的风险点不一样。

| 功能类型 | 例子 | 主要风险 |
| --- | --- | --- |
| 创建 | 创建用户、创建应用、创建迭代 | 重复数据、默认值、必填字段、关联对象是否存在 |
| 更新 | 修改用户、修改应用配置 | 资源不存在、部分更新、重复校验排除自己、并发覆盖 |
| 删除 | 删除用户、删除应用 | 是否允许删除、是否软删除、是否有关联数据 |
| 详情 | 查询用户详情、应用详情 | 资源不存在、越权访问、是否需要关联数据 |
| 列表 | 用户列表、应用列表 | 分页、排序、过滤条件、性能、空条件 |
| 搜索 | 搜索用户、搜索仓库 | 空关键词、模糊匹配范围、最大返回数量 |
| 操作 | 启用、禁用、发布、部署、同步 | 当前状态是否允许、外部服务失败、幂等性 |

如果你说不清功能类型，先不要写代码。先把接口行为写成一句话：

```text
谁，在什么条件下，对哪个资源，做什么操作，成功后产生什么结果。
```

例如：

```text
管理员在用户未被删除时，可以修改用户角色；成功后用户角色变更并更新 updatedAt。
```

## 1. 需求分析 Checklist

写任何后端功能前，先回答这些问题。

### 1.1 资源是什么

- 这个功能操作的核心资源是什么？例如 User、Application、Iteration、Requirement。
- 资源是否已经有 Entity？
- 是否要新增表？
- 是否只是读取外部服务，不落库？
- 是否涉及多个资源之间的关系？

### 1.2 谁可以操作

- 是否需要登录？
- 所有登录用户都能操作，还是只有管理员、负责人、创建人可以操作？
- 查询列表时，是否只能看自己的数据？
- 操作详情时，是否要校验当前用户对这条数据有权限？

### 1.3 输入从哪里来

- Route 参数：例如 `/api/users/{id}`。
- Query 参数：例如 `pageIndex`、`pageSize`、`name`。
- Body 参数：例如创建、更新表单。
- Header 参数：例如 Authorization、租户、客户端版本。
- 当前用户信息：例如 userId、role、name。

### 1.4 成功后返回什么

- 返回详情 DTO，还是只返回 `true`？
- 返回字段是否给前端足够展示？
- 是否暴露了不该暴露的字段？例如 passwordHash、token secret、内部配置。
- 时间字段使用什么格式？
- 枚举字段是否需要同时返回 code 和 name？

### 1.5 失败时有哪些情况

至少列出这些失败分支：

- 参数不合法。
- 未登录。
- 无权限。
- 资源不存在。
- 资源重复。
- 当前状态不允许操作。
- 关联资源不存在。
- 外部系统失败。
- 数据库写入失败。

后端不能只处理“正常路径”。真正的代码标准，主要体现在这些边界路径是否清楚。

## 2. 标准文件清单

新增一个业务模块时，通常需要这些文件。

```text
src/Domain/Entities/<Entity>.cs

src/Application/<Feature>/
├── <CreateFeature>Request.cs
├── <UpdateFeature>Request.cs
├── <Feature>QueryRequest.cs
├── <Feature>Response.cs
├── <CreateFeature>RequestValidator.cs
├── <UpdateFeature>RequestValidator.cs
├── I<Feature>Repository.cs
└── <Feature>Service.cs

src/Infrastructure/Persistence/Configurations/<Entity>Configuration.cs
src/Infrastructure/Persistence/<Feature>Repository.cs

src/Api/Controllers/<Feature>Controller.cs

tests/UnitTests/...
tests/IntegrationTests/...
```

不是每个功能都需要全部文件。简单查询可能不需要 Create/Update Request；不落库的功能可能不需要 Entity 和 Repository。但你必须显式判断，而不是忘记。

## 3. 推荐开发顺序

### Step 1：先定义 API 行为

先写清楚接口，不要急着写数据库。

你要明确：

- URL 是什么。
- HTTP Method 是什么。
- 是否需要 `[Authorize]`。
- 输入来自 Route、Query 还是 Body。
- 成功返回什么 DTO。
- 失败返回哪些错误码。

推荐 REST 风格：

| 行为 | Method | URL |
| --- | --- | --- |
| 创建 | POST | `/api/users` |
| 更新 | PUT/PATCH | `/api/users/{id}` |
| 删除 | DELETE | `/api/users/{id}` |
| 详情 | GET | `/api/users/{id}` |
| 列表 | GET | `/api/users` |
| 操作 | POST | `/api/users/{id}/disable` |

当前旧系统里有一些接口使用 `create`、`update`、`detail/:id`、`list` 这类命名。迁移到 .NET 新项目时，如果没有兼容前端的压力，优先使用更标准的 REST 风格；如果已有前端依赖旧路径，就先保持兼容，并在文档里说明原因。

### Step 2：定义 Domain Entity

Entity 表示数据库里长期存在的业务对象。它不是前端表单，也不是接口返回。

Entity 里通常有：

- 主键：`Id`。
- 业务唯一键：例如 `UserId`、`AppKey`。
- 业务字段：例如 `Name`、`Status`。
- 审计字段：例如 `CreatedAt`、`UpdatedAt`。
- 关联字段：例如 `ManagerUserId`、`OwnerId`。

Checklist：

- 字段是否真的需要持久化？
- 字段是否可以为空？
- 字符串最大长度是多少？
- 是否需要唯一索引？
- 是否需要普通索引？
- 状态字段是否用统一枚举或常量？
- 创建时间、更新时间由后端生成，不要相信前端传入。
- 密码、token、密钥不能明文保存。

### Step 3：定义 Request DTO

Request DTO 表示“前端可以传什么”。不要直接把 Entity 暴露给前端提交。

创建请求和更新请求要分开：

- `CreateXxxRequest`：创建时必填字段通常更多。
- `UpdateXxxRequest`：更新时很多字段可以为空，表示不修改。
- `XxxQueryRequest`：列表查询、分页、筛选条件。

Checklist：

- 创建和更新是否分成不同 DTO？
- 是否避免让前端传 `Id`、`CreatedAt`、`UpdatedAt`、`Creator` 等后端生成字段？
- 可选字段用 nullable 表达清楚，例如 `string?`、`int?`。
- 不要在 DTO 里写复杂业务逻辑。
- Query DTO 要有分页参数时，明确默认值和最大值。

### Step 4：定义 Response DTO

Response DTO 表示“后端允许前端看到什么”。不要直接返回 Entity。

Checklist：

- 是否隐藏敏感字段？例如 passwordHash。
- 是否返回前端展示需要的冗余字段？例如 `RoleName`。
- 时间字段是否统一。
- 枚举是否只返回数字会让前端难读？是否需要同时返回名称？
- 列表项 DTO 和详情 DTO 是否需要分开？

如果详情比列表多很多字段，可以拆：

```text
UserListItemResponse
UserDetailResponse
```

如果差异不大，可以先用一个 `UserResponse`，避免过度拆分。

### Step 5：写 Validator

Validator 负责输入格式校验，不负责查数据库。

适合放在 Validator 的规则：

- 必填。
- 长度。
- 邮箱、手机号格式。
- 数字范围。
- 枚举是否合法。
- 数组数量上限。
- 字符串不能全是空格。

不适合放在 Validator 的规则：

- 手机号是否已存在。
- 用户是否有权限。
- 关联对象是否存在。
- 当前状态是否允许发布。

这些需要查数据库或依赖当前用户，应该放到 Service。

Checklist：

- 字符串是否考虑空格输入？
- 可选字段为空时是否跳过格式校验？
- 分页参数是否限制最大值？
- 枚举是否限制在合法集合？
- 错误信息是否能让前端直接提示？

### Step 6：定义 Repository Interface

Repository interface 放在 Application 层，表示业务需要哪些数据访问能力。Infrastructure 层负责实现它。

Repository interface 不应该直接暴露 EF Core 的 `DbContext` 或 `IQueryable` 给 Service。Service 应该只看到业务需要的方法。

推荐：

```text
FindByIdAsync
FindByUserIdAsync
ExistsByMobileOrUserIdAsync
PageAsync
Add
SaveChangesAsync
```

Checklist：

- 方法名是否表达业务意图，而不是数据库细节？
- 是否所有 async 方法都有 `CancellationToken`？
- 查询单条不存在时，返回 nullable，例如 `Task<User?>`。
- 查询列表时，返回只读集合，例如 `IReadOnlyList<T>`。
- 分页是否同时返回 items 和 totalCount？
- 更新时是否需要排除自己做唯一校验，例如 `excludingId`？
- 是否避免让 Service 拼复杂 SQL？

### Step 7：实现 Service

Service 是业务流程的中心。Controller 不应该承载业务判断。

Service 通常负责：

- 调用 Validator。
- 标准化输入，例如 Trim、空字符串转 null。
- 查询资源是否存在。
- 判断唯一性。
- 判断状态是否允许。
- 处理当前用户信息。
- 调用 Repository。
- 调用外部服务。
- 设置创建时间、更新时间。
- 把 Entity 转成 Response DTO。

Checklist：

- 是否先校验 Request？
- 是否处理空字符串和 Trim？
- 资源不存在时是否抛业务异常？
- 重复数据是否有稳定错误码？
- 更新唯一字段时是否排除当前记录？
- 是否只在 Service 里设置后端生成字段？
- 是否避免返回 Entity？
- 是否传递 `CancellationToken`？
- 多步写入是否需要事务？

### Step 8：实现 Infrastructure

Infrastructure 放技术细节：

- EF Core DbContext。
- Entity Configuration。
- Repository 实现。
- Redis、OSS、Git、DingTalk 等外部服务实现。

Checklist：

- 查询只读数据是否使用 `AsNoTracking()`？
- 字符串字段长度是否和 Validator 对齐？
- 唯一索引是否和业务唯一规则对齐？
- 是否给常用查询字段加索引？
- 是否避免一次性 Include 太多关联对象？
- 是否避免 N+1 查询？
- 是否把外部服务失败转换成业务可理解的异常？

### Step 9：实现 Controller

Controller 是 HTTP 适配层。它应该尽量薄。

Controller 负责：

- 声明路由。
- 声明 `[Authorize]` 或 `[AllowAnonymous]`。
- 接收 Route、Query、Body。
- 获取当前用户。
- 调用 Service。
- 返回 Service 的结果。

Controller 不应该：

- 写复杂业务判断。
- 直接访问 DbContext。
- 拼 SQL。
- 决定密码怎么加密。
- 手动包装统一响应，除非这个接口需要特殊响应。

Checklist：

- Route 是否稳定、清晰？
- Method 是否符合行为？
- 是否加了 `[Authorize]`？
- 匿名接口是否显式写 `[AllowAnonymous]`？
- 是否把 `CancellationToken` 传给 Service？
- 是否没有把 Entity 直接返回给前端？

### Step 10：补测试

后端测试不是只测“能不能成功”。更重要的是测边界。

最少要覆盖：

- 正常创建 / 更新 / 查询。
- 参数错误返回 400。
- 未登录返回 401。
- 无权限返回 403。
- 资源不存在。
- 重复数据。
- 状态不允许。
- 分页边界。

如果时间有限，优先补集成测试覆盖接口行为。因为对前端来说，最终依赖的是 HTTP 行为和响应结构。

## 4. CRUD 专项 Checklist

### 4.1 创建接口

创建接口必须检查：

- 必填字段是否校验。
- 字符串是否 Trim。
- 空字符串是否转成 null 或拒绝。
- 唯一字段是否检查重复。
- 关联资源是否存在。
- 当前用户是否有创建权限。
- 默认状态是否由后端设置。
- 创建人、创建时间是否由后端设置。
- 是否返回创建后的 Response DTO。

常见坑：

- 前端不传字段时后端用了默认值，导致脏数据入库。
- 只在前端校验唯一性，后端没有校验。
- 密码、token 等敏感字段明文保存。
- 创建时允许前端传 `CreatedAt`、`Status` 等危险字段。

### 4.2 更新接口

更新接口必须检查：

- 资源是否存在。
- 当前用户是否有权限修改。
- 部分更新时，null 表示“不修改”还是“清空”。
- 唯一字段变更时，是否排除自己。
- 状态是否允许修改。
- 是否更新 `UpdatedAt`。
- 是否避免覆盖不可修改字段。

常见坑：

- 把前端传入对象直接覆盖 Entity，导致未传字段被清空。
- 唯一校验没有排除自己，导致不改手机号也报“手机号已存在”。
- 允许普通用户修改角色、状态等敏感字段。

### 4.3 删除接口

删除前先决定是硬删除还是软删除。

硬删除：

- 数据从数据库删除。
- 适合临时数据、无审计要求的数据。
- 风险是历史记录和关联数据可能丢失。

软删除：

- 设置 `DeletedAt`、`IsDeleted` 或状态。
- 适合业务主数据。
- 查询时必须默认过滤已删除数据。

删除接口必须检查：

- 资源是否存在。
- 当前用户是否有权限删除。
- 是否有关联数据阻止删除。
- 是否需要软删除。
- 删除是否幂等。

### 4.4 详情接口

详情接口必须检查：

- 资源不存在时返回稳定业务错误。
- 当前用户是否有权限查看。
- 是否需要加载关联数据。
- 是否隐藏敏感字段。
- 是否避免返回过大的对象图。

### 4.5 列表接口

列表接口必须检查：

- `pageIndex` 最小为 1。
- `pageSize` 有最大值，例如 100。
- 默认排序稳定，例如按 `Id DESC` 或 `CreatedAt DESC`。
- 空筛选条件是否忽略。
- 字符串搜索是否 Trim。
- 是否需要按当前用户过滤数据范围。
- 是否返回 `totalCount`。

常见坑：

- 不限制 `pageSize`，前端传 100000 造成慢查询。
- 没有稳定排序，翻页时数据重复或丢失。
- 搜索条件全是空格，查出异常结果。

### 4.6 操作类接口

例如启用、禁用、发布、部署、同步。

操作类接口必须检查：

- 当前状态是否允许操作。
- 重复点击是否幂等。
- 是否需要记录操作人和操作时间。
- 是否调用外部服务。
- 外部服务失败时本地数据怎么办。
- 多步更新是否需要事务。

## 5. 提测前 Definition of Done

一个后端功能写完，至少满足这些条件再提测。

### 5.1 代码结构

- Controller 只做 HTTP 适配。
- Service 承载业务流程。
- Repository interface 放在 Application。
- Repository 实现放在 Infrastructure。
- Entity 不直接暴露给前端。
- Request 和 Response DTO 分开。

### 5.2 输入和边界

- 必填、长度、格式、枚举、分页都有校验。
- 空字符串、全空格、null 已明确处理。
- 资源不存在有明确错误。
- 重复数据有明确错误。
- 状态不允许有明确错误。
- 当前用户和权限已检查。

### 5.3 数据库

- 字段 nullable、长度、索引与业务规则一致。
- 唯一约束不只依赖代码校验。
- 只读查询尽量 `AsNoTracking()`。
- 多步写入有事务判断。
- 没有把敏感信息明文落库。

### 5.4 接口体验

- 成功响应结构稳定。
- 错误响应结构稳定。
- 错误码稳定，前端能识别。
- 分页结构稳定。
- Swagger 或 API 文档能看懂。

### 5.5 可排查性

- 关键失败分支有日志。
- 未知异常会记录 exception。
- 响应里能带 requestId。
- 外部服务调用失败能定位到服务名和关键参数。

### 5.6 测试

- 正常路径有测试。
- 参数错误有测试。
- 未登录或无权限有测试。
- 不存在、重复、状态不允许有测试。
- 分页边界有测试。

## 6. 新功能最小模板

如果不知道从哪里开始，可以按这个顺序建文件：

```text
1. Domain/Entities/Xxx.cs
2. Application/Xxx/CreateXxxRequest.cs
3. Application/Xxx/UpdateXxxRequest.cs
4. Application/Xxx/XxxQueryRequest.cs
5. Application/Xxx/XxxResponse.cs
6. Application/Xxx/CreateXxxRequestValidator.cs
7. Application/Xxx/UpdateXxxRequestValidator.cs
8. Application/Xxx/IXxxRepository.cs
9. Application/Xxx/XxxService.cs
10. Infrastructure/Persistence/Configurations/XxxConfiguration.cs
11. Infrastructure/Persistence/XxxRepository.cs
12. Api/Controllers/XxxController.cs
13. tests/IntegrationTests/XxxApiTests.cs
```

每建一个文件，都问一句：

```text
这个文件的职责是否单一？
它是否只依赖自己应该依赖的层？
它是否暴露了不该暴露的技术细节或敏感字段？
```

