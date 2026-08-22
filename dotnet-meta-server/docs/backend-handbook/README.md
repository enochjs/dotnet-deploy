# .NET 后端开发查询手册

这组文档给“有前端经验，但刚开始写后端”的同学使用。目标不是把 ASP.NET Core、EF Core、认证授权一次性讲完，而是让你在开发一个后端功能时知道：

- 需要补哪些文件。
- 每一层负责什么。
- 参数、异常、权限、数据库、日志、测试有哪些必查边界。
- 写完之后怎么判断这个功能是否达到了后端代码标准。

阅读时请把它当成“开发前检查单 + 开发中速查表 + 提测前自检表”。

## 适用项目

当前项目采用接近 Clean Architecture 的分层：

```text
dotnet-meta-server/
├── src/
│   ├── Api/              # HTTP 入口：Controller、Middleware、Auth、响应包装
│   ├── Application/      # 用例层：Request/Response、Validator、Service、Repository interface
│   ├── Domain/           # 领域层：Entity、核心业务概念
│   └── Infrastructure/   # 基础设施：EF Core、Repository 实现、外部服务实现
└── tests/
    ├── UnitTests/        # 单元测试
    └── IntegrationTests/ # 集成测试
```

可以参考这些现有文件理解风格：

- `src/Api/Program.cs`
- `src/Api/Middleware/ExceptionHandlingMiddleware.cs`
- `src/Api/Controllers/AuthController.cs`
- `src/Application/Users/UserService.cs`
- `src/Application/Users/CreateUserRequest.cs`
- `src/Application/Users/CreateUserRequestValidator.cs`
- `src/Application/Users/IUserRepository.cs`
- `src/Infrastructure/Persistence/MetaServerDbContext.cs`
- `src/Infrastructure/Persistence/Configurations/UserConfiguration.cs`

## 文档目录

### 1. 功能开发流程

文件：`01-feature-development-playbook.md`

适合在“我要新增一个业务功能 / 一个 CRUD / 一个查询列表 / 一个操作按钮接口”时从头照着看。它会告诉你：

- 需求分析时先问哪些问题。
- Domain、Application、Infrastructure、Api 各自要新增什么。
- CRUD、详情、列表、搜索、状态变更分别要注意什么。
- 写完功能后如何自检。

### 2. 后端边界速查

文件：`02-backend-boundary-checklists.md`

适合遇到某类具体问题时快速查。例如：

- 参数验证应该放在哪里。
- DTO 怎么设计。
- 业务异常和系统异常怎么区分。
- Auth、当前用户、权限应该怎么处理。
- Repository interface 的参数和返回值有什么规范。
- 分页、排序、空字符串、大小写、重复数据怎么处理。
- 日志和测试要覆盖哪些点。

## 推荐使用方式

开发新功能时按这个顺序使用：

```text
1. 先看 01 的“开发总流程”
2. 按功能类型看对应小节：创建、更新、删除、列表、详情、操作类接口
3. 写代码过程中随时查 02 的专项 checklist
4. 提测前用 01 最后的 Definition of Done 自检
```

## 后端和前端最大的思维差异

前端通常更关心：

- 页面状态是否正确。
- 交互是否顺。
- 数据是否展示出来。
- 用户是否能完成操作。

后端还必须额外关心：

- 不可信输入：请求里的任何字段都可能为空、超长、伪造、类型不对。
- 并发：两个请求可能同时修改同一份数据。
- 数据一致性：写了一半失败怎么办，关联数据要不要一起回滚。
- 权限边界：用户能不能看、能不能改、能不能操作这个资源。
- 可追踪性：线上出问题时能不能通过 requestId、日志、错误码定位。
- 长期维护：接口返回结构、错误码、数据库字段一旦被前端或外部系统依赖，就不能随便改。

所以后端功能不是“Controller 收到参数，然后查库返回”这么简单。一个标准后端功能至少要同时考虑：

```text
输入是否合法
当前用户是谁
用户有没有权限
资源是否存在
业务状态是否允许操作
数据库读写是否一致
异常是否能被前端识别
日志是否能排查问题
测试是否覆盖关键分支
```

## 一句话原则

写后端功能时，不要只问“正常情况能不能跑通”，要问：

```text
空值怎么办？
不存在怎么办？
重复怎么办？
越权怎么办？
状态不允许怎么办？
并发怎么办？
外部服务失败怎么办？
前端如何识别失败原因？
线上如何定位这次请求？
```

