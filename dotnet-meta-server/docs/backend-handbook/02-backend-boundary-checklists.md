# 02. 后端边界速查 Checklist

这份文档按后端常见能力分类。开发过程中遇到参数、异常、Auth、Repository、数据库、日志、测试等问题时，可以直接跳到对应章节。

## 1. 参数验证

参数验证分两层：

```text
格式校验：Validator
业务校验：Service
```

### 1.1 Validator 负责什么

Validator 只检查“不需要查数据库、不依赖当前用户”的规则。

适合：

- 必填：`NotEmpty()`。
- 最大长度：`MaximumLength(64)`。
- 最小长度：`MinimumLength(6)`。
- 格式：手机号、邮箱、URL。
- 数字范围：页码、金额、数量。
- 枚举值是否合法。
- 数组是否为空、是否超过最大数量。

不适合：

- 名称是否重复。
- 用户是否存在。
- 当前用户是否能修改这条数据。
- 当前状态是否允许提交。
- 外部服务里是否存在某个 Git 仓库。

### 1.2 字符串处理 Checklist

- 必填字符串是否拒绝 `null`、`""`、`"   "`？
- 可选字符串为空时，是保存 null，还是保存空字符串？
- 入库前是否 `Trim()`？
- 查询关键词是否 `Trim()`？
- 是否限制最大长度？
- 是否区分大小写？
- 是否允许特殊字符？

建议：

```text
可选字符串：空白统一转 null
必填字符串：空白直接校验失败
查询字符串：先 Trim，空白时返回空结果或忽略条件
```

### 1.3 数字处理 Checklist

- id 是否必须大于 0？
- pageIndex 是否至少为 1？
- pageSize 是否限制最大值？
- 金额、数量是否允许 0？
- 排序字段是否只能从白名单选择？

分页建议：

```text
pageIndex = Math.Max(query.PageIndex, 1)
pageSize = Math.Clamp(query.PageSize, 1, 100)
```

### 1.4 枚举处理 Checklist

- 前端传来的枚举值是否在合法集合里？
- 以后新增枚举值时，老前端是否能兼容？
- Response 是否需要返回枚举名称？
- 数据库里是否允许未知状态？

推荐做法：

```text
Application 层提供 XxxStatuses / XxxTypes 这类常量帮助类
Validator 调用 IsValid
Response 同时返回 Status 和 StatusName
```

## 2. DTO 设计

DTO 是 API 合同。它比 Entity 更接近前端，比数据库更稳定。

### 2.1 Request DTO

Request DTO 的职责：

- 描述前端能传什么。
- 表达字段是否可选。
- 给 Validator 提供校验目标。

Checklist：

- 创建和更新是否分开？
- Body DTO 和 Query DTO 是否分开？
- 是否避免让前端传后端生成字段？
- 字段命名是否稳定？
- nullable 是否准确？
- 是否避免复用 Entity？

### 2.2 Response DTO

Response DTO 的职责：

- 描述前端能看到什么。
- 隐藏后端内部字段。
- 屏蔽数据库结构变化。

Checklist：

- 是否隐藏密码、token、secret、内部配置？
- 是否隐藏软删除字段？
- 是否返回前端需要展示的名称字段？
- 是否避免循环引用？
- 是否避免返回超大对象？

### 2.3 DTO 常见反例

反例 1：直接返回 Entity。

问题：

- 数据库字段变动会影响前端。
- 容易暴露敏感字段。
- 关联对象可能无限展开。

反例 2：创建、更新共用一个 DTO。

问题：

- 创建必填和更新可选的规则不同。
- 更新时容易误清空字段。

反例 3：DTO 里塞业务逻辑。

问题：

- DTO 应该是数据形状，不应该承担流程判断。

## 3. 异常处理

后端异常要让前端可识别、让日志可排查、让用户提示不泄露内部细节。

### 3.1 异常分类

| 类型 | HTTP 状态 | 例子 | 前端处理 |
| --- | --- | --- | --- |
| 参数错误 | 400 | 手机号格式错误 | 表单提示 |
| 未登录 | 401 | token 缺失或过期 | 跳登录 |
| 无权限 | 403 | 普通用户修改管理员字段 | 提示无权限 |
| 资源不存在 | 400/404 | 用户不存在 | 提示资源不存在 |
| 业务规则错误 | 400 | 状态不允许发布 | 业务提示 |
| 外部服务失败 | 400/502 | Git 服务不可用 | 提示稍后重试或联系管理员 |
| 未知异常 | 500 | 空引用、数据库异常 | 通用错误提示 |

当前项目里可以参考：

- `Application.Common.BusinessRuleException`
- `Application.Common.RequestValidationException`
- `Api.Middleware.ExceptionHandlingMiddleware`
- `Api.Responses.ApiResponse`

### 3.2 业务异常 Checklist

业务异常应该包含：

- 稳定错误码。
- 用户可理解的错误消息。
- 必要时记录日志上下文。

推荐错误码：

```text
USER_NOT_FOUND
USER_MOBILE_EXISTS
APPLICATION_NOT_FOUND
APPLICATION_APP_KEY_EXISTS
STATUS_NOT_ALLOWED
PERMISSION_DENIED
```

注意：

- 错误码要稳定，不要今天叫 `USER_NOT_FOUND`，明天改成 `USER_NOT_EXIST`。
- 错误信息可以优化，但错误码改动会影响前端判断。
- 不要把数据库异常原文直接返回给前端。

### 3.3 什么时候抛异常，什么时候返回 null

Repository：

```text
找不到数据：返回 null
```

Service：

```text
业务要求必须存在：把 null 转成业务异常
```

Controller：

```text
不处理业务异常，交给全局异常中间件
```

这样每层职责更清楚。

### 3.4 未知异常处理

未知异常必须：

- 记录完整 exception。
- 返回统一 500 结构。
- 不把堆栈、SQL、连接串、密钥返回给前端。
- 响应里带 requestId。

## 4. Auth 和权限

Auth 分两件事：

```text
Authentication：你是谁
Authorization：你能不能做这件事
```

### 4.1 Authentication

认证负责从 token 中解析当前用户。

Checklist：

- 登录接口是否 `[AllowAnonymous]`？
- 需要登录的接口是否 `[Authorize]`？
- token 是否校验签名、过期时间、issuer、audience？
- 当前用户是否能从 `HttpContext.User` 取到？
- 用户不存在或被禁用时怎么办？

当前项目里可以参考：

- `src/Api/Auth/JwtTokenService.cs`
- `src/Api/Auth/CurrentUserAccessor.cs`
- `src/Api/Auth/JwtBearerOptionsSetup.cs`
- `src/Api/Controllers/AuthController.cs`

### 4.2 Authorization

授权负责判断当前用户是否能操作某个资源。

常见权限：

- 角色权限：管理员、普通用户。
- 资源权限：只能操作自己负责的应用。
- 状态权限：只有某些状态允许操作。
- 字段权限：普通用户不能修改角色、状态等敏感字段。

Checklist：

- 是否所有接口都明确了登录要求？
- 是否区分“能看列表”和“能看详情”？
- 是否区分“能编辑”和“能删除”？
- 是否检查当前用户对这条资源的权限，而不只是检查是否登录？
- 是否避免相信前端传来的 creator、owner、role？

### 4.3 当前用户处理

当前用户信息应该来自 token 或服务端上下文，而不是前端 Body。

例如创建时：

```text
creator = currentUser.UserId
creatorName = currentUser.Name
```

不要让前端提交 creator，因为前端可以伪造。

## 5. Repository Interface

Repository 是 Application 层和数据库实现之间的边界。

### 5.1 方法设计原则

Repository 方法应该表达业务需要。

推荐：

```text
FindByIdAsync
FindByCodeAsync
ExistsByNameAsync
PageAsync
Add
Remove
SaveChangesAsync
```

谨慎：

```text
GetQueryable
ExecuteRawSql
UpdateAnyField
```

这些方法会把数据库细节泄漏到 Service。

### 5.2 参数 Checklist

- 是否传入 `CancellationToken`？
- 查询 id 是否使用明确类型？
- 查询条件是否使用 QueryRequest？
- 唯一校验是否支持 excludingId？
- 是否避免传整个 Controller Request 到 Infrastructure？

### 5.3 返回值 Checklist

- 单条查询找不到时是否返回 nullable？
- 列表是否返回 `IReadOnlyList<T>`？
- 分页是否返回 items + totalCount？
- 是否避免返回 `IQueryable<T>`？
- 是否避免返回 EF Core tracking 的对象给不需要修改的流程？

### 5.4 SaveChanges 放在哪里

当前项目里 Repository 有 `Add` 和 `SaveChangesAsync`。这代表 Service 可以控制一次业务操作什么时候提交。

好处：

- 一个 Service 方法里多个 Add/Update 可以一起提交。
- 以后需要事务时更容易组织。

注意：

- 不要忘记调用 `SaveChangesAsync`。
- 查询方法不应该偷偷 SaveChanges。

## 6. Service 层边界

Service 是后端功能最重要的层。

### 6.1 Service 应该做

- 调 Validator。
- 标准化参数。
- 查资源。
- 判权限。
- 判状态。
- 判重复。
- 组织数据库写入。
- 调外部服务。
- 转 Response。

### 6.2 Service 不应该做

- 直接读取 HTTP Header。
- 直接依赖 Controller。
- 直接返回 ActionResult。
- 拼接前端展示文案的大段逻辑。
- 暴露 EF Core IQueryable 给外部。

### 6.3 标准 Service 流程

```text
1. Validate request
2. Normalize input
3. Load current resource
4. Check not found
5. Check permission
6. Check business rules
7. Apply changes
8. Save changes
9. Return response DTO
```

如果某个 Service 方法很长，先看是不是混了太多职责：

- 查询条件构造能不能放 Repository。
- Entity 到 Response 的转换能不能抽私有方法。
- 外部服务调用能不能封装成独立接口。
- 多个操作是不是应该拆成多个私有步骤。

## 7. Controller 边界

Controller 是入口，不是业务层。

### 7.1 Controller Checklist

- 是否有 `[ApiController]`？
- Route 是否清晰？
- 需要登录的接口是否 `[Authorize]`？
- 公开接口是否 `[AllowAnonymous]`？
- Body、Query、Route 是否区分清楚？
- 是否传递 `CancellationToken`？
- 是否没有写复杂业务判断？
- 是否没有直接访问 DbContext？

### 7.2 Controller 常见反例

反例：

```text
Controller 里查数据库、判断重复、修改 Entity、保存数据库。
```

问题：

- 业务无法复用。
- 难写单元测试。
- Controller 越来越胖。
- 后续换入口时逻辑无法复用。

推荐：

```text
Controller -> Service -> Repository Interface -> Repository Implementation
```

## 8. 数据库和 EF Core

### 8.1 Entity Configuration Checklist

- 表名是否明确？
- 主键是否明确？
- 字符串长度是否明确？
- 必填字段是否 `IsRequired()`？
- 唯一索引是否配置？
- 常用筛选字段是否配置索引？
- 金额是否指定精度？
- 时间字段是否统一使用 UTC？
- 软删除字段是否有查询约定？

### 8.2 查询 Checklist

- 只读查询是否 `AsNoTracking()`？
- 是否只 Include 必要关联？
- 是否可能产生 N+1？
- 是否有稳定排序？
- 是否限制返回数量？
- 模糊查询字段是否有性能风险？

### 8.3 写入 Checklist

- 是否由后端设置 CreatedAt、UpdatedAt？
- 是否需要事务？
- 多表写入失败时是否会产生半完成数据？
- 是否依赖数据库唯一约束兜底？
- 是否处理并发更新？

### 8.4 Migration Checklist

- 是否新增了 migration？
- migration 是否只包含本次需求相关变更？
- 是否会删除生产数据？
- 字段从 nullable 改 non-null 时，旧数据怎么办？
- 新增唯一索引时，历史重复数据怎么办？

## 9. 分页、搜索、排序

### 9.1 分页标准

统一使用：

```text
pageIndex
pageSize
totalCount
items
```

Checklist：

- pageIndex 小于 1 时是否修正？
- pageSize 是否限制最大值？
- 是否返回 totalCount？
- 是否有稳定排序？
- 空列表是否返回 `items: []`？

### 9.2 搜索标准

Checklist：

- key 是否 Trim？
- key 为空时是返回空列表，还是返回全部？
- 是否限制最大返回数？
- 搜索字段是否明确？
- 是否需要大小写不敏感？

前端同学特别要注意：搜索框为空时，后端不能随便全表扫描。要么明确返回空，要么走分页列表接口。

### 9.3 排序标准

Checklist：

- 排序字段是否白名单？
- 默认排序是否稳定？
- 前端传非法排序字段时怎么办？
- 是否避免把数据库字段名完全暴露给前端？

## 10. 外部服务

例如 Git、DingTalk、OSS、Redis、Rancher。

Checklist：

- 配置是否来自 Options，不写死在代码里？
- 密钥是否不提交到 git？
- 超时时间是否明确？
- 失败是否有业务可理解的错误？
- 日志是否记录服务名、操作名、关键业务 id？
- 是否避免记录 token、secret、password？
- 是否需要重试？
- 是否需要幂等？

外部服务失败时，不要直接把底层异常返回给前端。前端只需要知道“什么失败了、是否可以重试、requestId 是什么”。

## 11. 日志和 requestId

日志不是越多越好，而是要能定位问题。

### 11.1 必须记录

- 未知异常。
- 外部服务失败。
- 关键业务操作失败。
- 权限拒绝，如果需要审计。
- 慢请求，如果后续加入性能监控。

### 11.2 不应该记录

- 密码。
- token。
- secret。
- 完整 Authorization Header。
- 大量请求 body。
- 个人敏感信息，除非业务和合规允许。

### 11.3 日志 Checklist

- 是否包含 requestId？
- 是否包含业务 id？
- 是否包含当前用户 id？
- 是否包含外部服务名？
- 是否包含异常对象，而不只是异常 message？

## 12. 测试

### 12.1 单元测试适合测什么

- Validator 规则。
- Service 的业务分支。
- 枚举 helper。
- 纯函数转换逻辑。

### 12.2 集成测试适合测什么

- HTTP 状态码。
- 统一响应结构。
- Auth 行为。
- 参数错误响应。
- 数据库读写结果。
- 分页结果。

### 12.3 每个功能至少测这些

- 正常成功。
- 参数错误。
- 未登录。
- 资源不存在。
- 重复数据。
- 状态不允许。
- 分页边界。

如果这个功能涉及权限，还要测：

- 无权限不能操作。
- 只能操作自己的数据。
- 管理员可以操作。

## 13. 前端转后端常见误区

### 13.1 “前端已经校验了，后端不用校验”

不对。前端校验只是用户体验，后端校验才是安全边界。任何人都可以绕过页面直接发请求。

### 13.2 “接口只给我们自己的前端用，所以可以相信参数”

不对。自己的前端也可能有 bug，用户也可能手动改请求，线上旧版本前端也可能继续调用接口。

### 13.3 “先返回 Entity，后面再改 DTO”

谨慎。前端一旦依赖 Entity 字段，后面数据库结构就很难改。Response DTO 是保护后端演进空间的边界。

### 13.4 “删除就是 delete”

不一定。业务数据通常需要审计、恢复、历史关联。删除前必须判断硬删除还是软删除。

### 13.5 “报错直接 throw new Exception”

不建议。业务错误要有稳定错误码。未知异常才走通用 500。

### 13.6 “列表接口先不分页”

不建议。小数据会变大，接口会被复用。列表默认就应该考虑分页、排序、筛选和最大 pageSize。

### 13.7 “权限前端隐藏按钮就行”

不对。隐藏按钮只是体验，后端必须判断权限。用户可以直接调用接口。

## 14. 最终自检清单

提交前从上到下打一遍：

- [ ] 我知道这个功能属于创建、更新、删除、详情、列表、搜索还是操作类接口。
- [ ] 我定义了清晰的 Request DTO。
- [ ] 我定义了清晰的 Response DTO，没有直接返回 Entity。
- [ ] 我写了 Validator，并覆盖必填、长度、格式、枚举、分页。
- [ ] 我在 Service 里处理了资源不存在、重复、权限、状态等业务边界。
- [ ] 我没有在 Controller 里写复杂业务逻辑。
- [ ] 我定义了 Repository interface，而不是让 Service 直接依赖 EF Core。
- [ ] 我给 Repository async 方法传递了 CancellationToken。
- [ ] 我处理了空字符串、Trim、null。
- [ ] 我限制了 pageSize，并提供稳定排序。
- [ ] 我没有把敏感字段返回给前端或写入日志。
- [ ] 我为业务失败提供了稳定错误码。
- [ ] 我确认了是否需要数据库索引、唯一约束、migration。
- [ ] 我确认了是否需要事务。
- [ ] 我补了正常路径和关键失败路径测试。
- [ ] 我能通过 requestId 和日志排查线上失败。

