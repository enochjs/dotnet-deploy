# Day 03 Step-by-Step - EF Core 实体建模和 PostgreSQL 表结构

这份文档是 `day-03.md` 的跟做版。今天把原 NestJS 项目的业务对象，重新建成一个全新的 .NET 后端数据模型。

重点先说清楚：这是新项目，不是旧库表结构的一比一迁移。原业务代码只用来理解“系统有哪些对象、字段和关系”，具体实现按现代 .NET、EF Core、PostgreSQL 的规范来写。

原业务源码只作为功能参考；具体参考文件清单放在文档最后。

## 0. 今天最终要得到什么

完成后，你会在 Day 02 的项目上新增这些能力：

- `Infrastructure` 安装 EF Core 和 PostgreSQL provider。
- `Domain` 中有 14 个核心业务实体。
- `Infrastructure/Persistence` 中有 `MetaServerDbContext`。
- 每个实体有独立的 `IEntityTypeConfiguration<T>`。
- 表名、列名采用新的 `snake_case` 规范，例如 `applications`、`pipeline_jobs`、`created_at`。
- 时间字段使用 `DateTimeOffset` 表达 UTC 时间点，数据库使用 `timestamp with time zone`。
- JSON 扩展字段使用 `jsonb`；形状稳定时优先建强类型 complex type + `ToJson()`，形状不稳定时才用 `JsonDocument?`。
- 常用查询字段有索引。
- 实体关系按 EF Core 标准表达，支持后续 `Include` 和业务查询。
- metadata 测试能验证核心表、主键、列名、JSON 类型、索引、关系都正确。

你最后需要确认三件事：

- `dotnet build` 成功。
- `dotnet test` 成功。
- `MetaServerDbContextMetadataTests` 通过。

## 1. 回到项目根目录

### Step 1.1 进入 `dotnet-meta-server`

执行：

```bash
cd /Users/fenghe/enochjs/study/dotnet-deploy/dotnet-meta-server
```

确认当前位置：

```bash
pwd
```

你应该看到：

```text
/Users/fenghe/enochjs/study/dotnet-deploy/dotnet-meta-server
```

### Step 1.2 确认 Day 02 代码能编译

执行：

```bash
dotnet build
```

验收：

- `dotnet build` 能看到 `Build succeeded.`

如果失败，先修 Day 02。Day 03 会新增 EF Core 模型，基础不干净时很难判断错误来源。测试命令今天统一放到最后再跑。

## 2. 从原项目抽取业务对象

### Step 2.1 查看原实体文件

执行：

```bash
find /Users/fenghe/workspace/devops/meta-server/src/core/entities -type f -name "*.ts" | sort
```

今天只抽取 14 个核心业务对象：

```text
Application              应用
SubApplication           子应用
User                     用户
Requirement              需求
Iteration                迭代
IntegrationRelease       集成发布
IntegrationReleaseApp    集成发布应用
PipelineTemplate         流水线模板
PipelineTemplateStage    流水线模板阶段
PipelineTemplateJob      流水线模板任务
Pipeline                 流水线执行实例
PipelineJob              流水线任务执行实例
Deploy                   发布记录
Monitor                  前端监控记录
```

### Step 2.2 只保留业务含义，不照搬旧表

今天做新项目设计，所以这些旧实现细节不照搬：

- 原表名是单数，例如 `application`；新表使用复数 `applications`。
- 原时间字段很多是 `text`；新表使用 `timestamp with time zone`。
- 原 `simple-json` 字段可能落成文本；新表使用 PostgreSQL 原生 `jsonb`。
- 原关系多数关闭外键；新项目正常建 EF 关系和数据库外键。
- 原字段 `creator`、`updater`；新项目使用更明确的 `created_by_user_id`、`updated_by_user_id`。

这些业务概念要保留：

- 应用和子应用。
- 应用、子应用和流水线模板的关联。
- 需求、迭代、用户的关联。
- 集成发布和派生迭代、子应用的关联。
- 流水线模板阶段和任务。
- 流水线执行实例和任务执行实例。
- 发布记录。
- 监控记录。

## 3. 安装 EF Core 依赖

### Step 3.1 给 Infrastructure 添加包

执行：

```bash
dotnet add src/Infrastructure/Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add src/Infrastructure/Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/Infrastructure/Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
```

这些包先这样理解：

- `Microsoft.EntityFrameworkCore`：EF Core 本体。
- `Npgsql.EntityFrameworkCore.PostgreSQL`：PostgreSQL provider。
- `Microsoft.EntityFrameworkCore.Design`：后续生成 migration 使用。

### Step 3.2 让 Api 引用 Infrastructure

执行：

```bash
dotnet add src/Api/Api.csproj reference src/Infrastructure/Infrastructure.csproj
```

`Api` 是组合根，负责注册数据库上下文、缓存、外部服务，所以 `Api -> Infrastructure` 是正常方向。

### Step 3.3 restore

执行：

```bash
dotnet restore
```

验收：

- restore 成功，没有红色错误。

## 4. 创建目录结构

### Step 4.1 创建 Domain 目录

执行：

```bash
mkdir -p src/Domain/Entities
mkdir -p src/Domain/Entities/Pipelines
mkdir -p src/Domain/Entities/Pipelines/Templates
```

### Step 4.2 创建 Infrastructure 目录

执行：

```bash
mkdir -p src/Infrastructure/Persistence
mkdir -p src/Infrastructure/Persistence/Configurations
mkdir -p src/Infrastructure/Persistence/Configurations/Pipelines
mkdir -p src/Infrastructure/Persistence/Configurations/Pipelines/Templates
```

验收：

```bash
find src/Domain src/Infrastructure -maxdepth 4 -type d
```

你应该能看到刚创建的目录。

## 5. 定义新项目建模规则

### Step 5.1 表名规则

新项目使用复数 `snake_case` 表名：

| 实体 | 表名 |
| --- | --- |
| Application | applications |
| SubApplication | sub_applications |
| User | users |
| Requirement | requirements |
| Iteration | iterations |
| IntegrationRelease | integration_releases |
| IntegrationReleaseApp | integration_release_apps |
| PipelineTemplate | pipeline_templates |
| PipelineTemplateStage | pipeline_template_stages |
| PipelineTemplateJob | pipeline_template_jobs |
| Pipeline | pipelines |
| PipelineJob | pipeline_jobs |
| Deploy | deploys |
| Monitor | monitors |

### Step 5.2 字段命名规则

C# 属性使用 .NET 风格：

```text
CreatedAt
UpdatedAt
CreatedByUserId
UpdatedByUserId
DingTalkUserId
ParentApplicationId
PipelineTemplateId
PipelineTemplateStageId
```

数据库列使用 `snake_case`：

```text
created_at
updated_at
created_by_user_id
updated_by_user_id
ding_talk_user_id
parent_application_id
pipeline_template_id
pipeline_template_stage_id
```

### Step 5.3 类型规则

今天统一按这些类型建模：

| 业务类型 | C# 类型 | PostgreSQL 类型 |
| --- | --- | --- |
| 自增主键 | int | integer identity |
| 运行实例主键 | Guid | uuid |
| 普通文本 | string | text |
| 有长度限制文本 | string | varchar(n) |
| 时间 | DateTimeOffset | timestamp with time zone |
| 稳定 JSON 结构 | complex type | jsonb |
| 非稳定 JSON 扩展 | JsonDocument? | jsonb |
| 状态枚举 | int | integer |
| 开关 | bool | boolean |

`Pipeline`、`PipelineJob`、`Deploy`、`Monitor` 是运行记录，主键用 `Guid`。其他主数据和配置实体主键用 `int`。

时间字段统一存 UTC。写入时用 `DateTimeOffset.UtcNow`，不要保存本机时区的 offset。PostgreSQL 的 `timestamp with time zone` 保存的是 UTC 时间点，不会保存“Asia/Shanghai”这类时区名称。

JSON 字段不要为了省事全部长期使用 `JsonDocument`。今天只有 `ranchers`、`variables`、`extra` 这种形状经常变化的扩展数据使用 `JsonDocument?`。如果后续某个 JSON 结构稳定下来，例如 Rancher 配置固定有 `env`、`host`、`tokenKey`，就把它提成强类型 class/record，并在 EF Core 配置里用 complex type + `ToJson()`。

集合导航统一使用非空集合。推荐写成只读集合属性：

```csharp
public ICollection<SubApplication> SubApplications { get; } = [];
```

这样符合 EF Core 对 collection navigation 的常见写法：集合本身永远不为 `null`，空集合表示没有关联数据，同时避免外部代码整体替换集合实例。

### Step 5.4 今天要修正的建模习惯

如果你从 TypeORM 或前端状态管理迁过来，容易写出“能跑但不够 .NET 标准”的实体。今天统一按下面的判断改：

| 写法 | 今天采用 | 原因 |
| --- | --- | --- |
| `User.DevelopRequirements`、`User.FollowRequirements` | 不放在 `User`，放在 `Requirement.Developers`、`Requirement.Followers` | 需求侧才是业务语义拥有者，用户侧反向集合只会增加耦合 |
| `Requirement.Users` | `Requirement.Developers` | 名字要表达业务角色，不要用泛泛的 Users |
| `IntegrationRelease.Applications` | `IntegrationRelease.ReleaseApps` | 避免和真正的 `Application` 聚合混淆 |
| `PipelineTemplate.Applications`、`PipelineTemplate.SubApplications` | 不放在模板里，只放在 `Application.PipelineTemplates`、`SubApplication.PipelineTemplates` | 模板是独立配置，应用选择模板；模板不应该反向依赖业务应用 |
| collection navigation 带 public setter | 只读 `ICollection<T> { get; } = []` | 集合永不为 null，且不鼓励整体替换集合 |
| 所有关系都只放 id | 关键关系同时放 FK 和 navigation | EF Core 学习重点之一就是关系建模和查询 |
| 所有 JSON 都用动态对象 | 稳定 JSON 用强类型，不稳定扩展才用 `JsonDocument?` | 让编译器帮助发现错误 |

## 6. 创建 Domain 实体

### Step 6.1 创建实体文件

创建这些文件：

```text
src/Domain/Entities/Application.cs
src/Domain/Entities/SubApplication.cs
src/Domain/Entities/User.cs
src/Domain/Entities/Requirement.cs
src/Domain/Entities/Iteration.cs
src/Domain/Entities/IntegrationRelease.cs
src/Domain/Entities/IntegrationReleaseApp.cs
src/Domain/Entities/Pipelines/Templates/PipelineTemplate.cs
src/Domain/Entities/Pipelines/Templates/PipelineTemplateStage.cs
src/Domain/Entities/Pipelines/Templates/PipelineTemplateJob.cs
src/Domain/Entities/Pipelines/Pipeline.cs
src/Domain/Entities/Pipelines/PipelineJob.cs
src/Domain/Entities/Deploy.cs
src/Domain/Entities/Monitor.cs
```

### Step 6.2 `Application.cs`

```csharp
using System.Text.Json;
using Domain.Entities.Pipelines.Templates;

namespace Domain.Entities;

public sealed class Application
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AppKey { get; set; } = string.Empty;
    public string ProjectType { get; set; } = "fe";
    public string? DeployKey { get; set; }
    public int GitId { get; set; }
    public string RegistryKey { get; set; } = "fe";
    public string GitName { get; set; } = string.Empty;
    public string GitRepo { get; set; } = string.Empty;
    public string MainBranch { get; set; } = "main";
    public string PreBranch { get; set; } = "pre";
    public string StageBranch { get; set; } = "stage";
    public string DevBranch { get; set; } = "dev";
    public int GitNamespaceId { get; set; }
    public string TriggerToken { get; set; } = string.Empty;
    public string OwnerUserId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Remark { get; set; }
    public JsonDocument? Ranchers { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? CreatedByUserName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string UpdatedByUserId { get; set; } = string.Empty;
    public string? UpdatedByUserName { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<SubApplication> SubApplications { get; } = [];
    public ICollection<PipelineTemplate> PipelineTemplates { get; } = [];
}
```

### Step 6.3 `SubApplication.cs`

```csharp
using System.Text.Json;
using Domain.Entities.Pipelines.Templates;

namespace Domain.Entities;

public sealed class SubApplication
{
    public int Id { get; set; }
    public int ParentApplicationId { get; set; }
    public Application ParentApplication { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string AppKey { get; set; } = string.Empty;
    public string? Platform { get; set; }
    public string? DeployKey { get; set; }
    public int GitId { get; set; }
    public string RegistryKey { get; set; } = "fe";
    public string GitName { get; set; } = string.Empty;
    public string GitRepo { get; set; } = string.Empty;
    public string MainBranch { get; set; } = "main";
    public string PreBranch { get; set; } = "pre";
    public string StageBranch { get; set; } = "stage";
    public string DevBranch { get; set; } = "dev";
    public string ProdSiteAddress { get; set; } = string.Empty;
    public string PreSiteAddress { get; set; } = string.Empty;
    public string StageSiteAddress { get; set; } = string.Empty;
    public string DevSiteAddress { get; set; } = string.Empty;
    public int GitNamespaceId { get; set; }
    public string TriggerToken { get; set; } = string.Empty;
    public string? Remark { get; set; }
    public string? PublicPath { get; set; }
    public bool UploadToOss { get; set; }
    public string AppType { get; set; } = "saas";
    public JsonDocument? Variables { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? CreatedByUserName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string UpdatedByUserId { get; set; } = string.Empty;
    public string? UpdatedByUserName { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<PipelineTemplate> PipelineTemplates { get; } = [];
}
```

### Step 6.4 `User.cs`

```csharp
namespace Domain.Entities;

public sealed class User
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? DingTalkUserId { get; set; }
    public string? ManagerUserId { get; set; }
    public string? ManagerDingTalkUserId { get; set; }
    public string? Email { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RealName { get; set; }
    public string Mobile { get; set; } = string.Empty;
    public int Role { get; set; }
    public int Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
```

### Step 6.5 `Requirement.cs`

```csharp
namespace Domain.Entities;

public sealed class Requirement
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
    public string DocumentUrl { get; set; } = string.Empty;
    public int? Priority { get; set; }
    public string? Remark { get; set; }
    public DateTimeOffset? OnlineAt { get; set; }
    public DateTimeOffset? SubmittedTestAt { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? CreatedByUserName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string UpdatedByUserId { get; set; } = string.Empty;
    public string? UpdatedByUserName { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<User> Developers { get; } = [];
    public ICollection<User> Followers { get; } = [];
    public ICollection<Iteration> Iterations { get; } = [];
}
```

### Step 6.6 `Iteration.cs`

```csharp
namespace Domain.Entities;

public sealed class Iteration
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    public int? SubApplicationId { get; set; }
    public SubApplication? SubApplication { get; set; }
    public int? IntegrationReleaseId { get; set; }
    public IntegrationRelease? IntegrationRelease { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string? SubApplicationName { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string? OriginalCommit { get; set; }
    public int Status { get; set; }
    public string? Remark { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? CreatedByUserName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string UpdatedByUserId { get; set; } = string.Empty;
    public string? UpdatedByUserName { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<Requirement> Requirements { get; } = [];
}
```

### Step 6.7 `IntegrationRelease.cs`

```csharp
namespace Domain.Entities;

public sealed class IntegrationRelease
{
    public int Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Branch { get; set; }
    public string? Remark { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? CreatedByUserName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<IntegrationReleaseApp> ReleaseApps { get; } = [];
}
```

### Step 6.8 `IntegrationReleaseApp.cs`

```csharp
namespace Domain.Entities;

public sealed class IntegrationReleaseApp
{
    public int Id { get; set; }
    public int IntegrationReleaseId { get; set; }
    public IntegrationRelease IntegrationRelease { get; set; } = null!;
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    public int SubApplicationId { get; set; }
    public SubApplication SubApplication { get; set; } = null!;
    public string AppKey { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string SubApplicationName { get; set; } = string.Empty;
    public int IterationId { get; set; }
    public Iteration Iteration { get; set; } = null!;
}
```

### Step 6.9 `PipelineTemplate.cs`

```csharp
namespace Domain.Entities.Pipelines.Templates;

public sealed class PipelineTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TemplateKey { get; set; }
    public int Status { get; set; } = 1;
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? CreatedByUserName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public string? UpdatedByUserName { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<PipelineTemplateStage> Stages { get; } = [];
}
```

### Step 6.10 `PipelineTemplateStage.cs`

```csharp
namespace Domain.Entities.Pipelines.Templates;

public sealed class PipelineTemplateStage
{
    public int Id { get; set; }
    public int PipelineTemplateId { get; set; }
    public PipelineTemplate PipelineTemplate { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public int Seq { get; set; }
    public ICollection<PipelineTemplateJob> Jobs { get; } = [];
}
```

### Step 6.11 `PipelineTemplateJob.cs`

```csharp
using System.Text.Json;

namespace Domain.Entities.Pipelines.Templates;

public sealed class PipelineTemplateJob
{
    public int Id { get; set; }
    public int PipelineTemplateStageId { get; set; }
    public PipelineTemplateStage PipelineTemplateStage { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string JobKey { get; set; } = string.Empty;
    public int StageSeq { get; set; }
    public JsonDocument? Extra { get; set; }
}
```

### Step 6.12 `Pipeline.cs`

```csharp
using System.Text.Json;
using Domain.Entities.Pipelines.Templates;

namespace Domain.Entities.Pipelines;

public sealed class Pipeline
{
    public Guid Id { get; set; }
    public string AppKey { get; set; } = string.Empty;
    public int IterationId { get; set; }
    public int RepoId { get; set; }
    public string RegistryKey { get; set; } = "fe";
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? CreatedByUserName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int Status { get; set; }
    public int StageSeq { get; set; } = -1;
    public int PipelineTemplateId { get; set; }
    public PipelineTemplate PipelineTemplate { get; set; } = null!;
    public string Branch { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? SwimLane { get; set; }
    public int? ForceUpdate { get; set; }
    public JsonDocument? Extra { get; set; }
    public ICollection<PipelineJob> Jobs { get; } = [];
}
```

### Step 6.13 `PipelineJob.cs`

```csharp
using System.Text.Json;

namespace Domain.Entities.Pipelines;

public sealed class PipelineJob
{
    public Guid Id { get; set; }
    public Guid PipelineId { get; set; }
    public Pipeline Pipeline { get; set; } = null!;
    public int StageSeq { get; set; }
    public string JobKey { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? UnitKey { get; set; }
    public JsonDocument? Extra { get; set; }
}
```

### Step 6.14 `Deploy.cs`

```csharp
using Domain.Entities.Pipelines;

namespace Domain.Entities;

public sealed class Deploy
{
    public Guid Id { get; set; }
    public Guid? PipelineId { get; set; }
    public Pipeline? Pipeline { get; set; }
    public string AppKey { get; set; } = string.Empty;
    public int? IterationId { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? CreatedByUserName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Env { get; set; } = string.Empty;
    public string? Version { get; set; }
    public bool? UseVpn { get; set; }
    public int? DeployType { get; set; }
    public string? SwimLane { get; set; }
    public string? IntegrationReleaseVersion { get; set; }
}
```

### Step 6.15 `Monitor.cs`

```csharp
namespace Domain.Entities;

public sealed class Monitor
{
    public Guid Id { get; set; }
    public string AppKey { get; set; } = string.Empty;
    public string Env { get; set; } = string.Empty;
    public string? Version { get; set; }
    public string? SourceUuid { get; set; }
    public string? TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Url { get; set; }
    public string? Browser { get; set; }
    public string? Message { get; set; }
    public string? Stack { get; set; }
    public int Status { get; set; }
    public string? Remark { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? ResolvedByUserId { get; set; }
    public string? ResolvedByUserName { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
}
```

## 7. 创建 DbContext

### Step 7.1 创建 `MetaServerDbContext.cs`

创建文件：

```text
src/Infrastructure/Persistence/MetaServerDbContext.cs
```

填入：

```csharp
using Domain.Entities;
using Domain.Entities.Pipelines;
using Domain.Entities.Pipelines.Templates;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class MetaServerDbContext(DbContextOptions<MetaServerDbContext> options)
    : DbContext(options)
{
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<SubApplication> SubApplications => Set<SubApplication>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Requirement> Requirements => Set<Requirement>();
    public DbSet<Iteration> Iterations => Set<Iteration>();
    public DbSet<IntegrationRelease> IntegrationReleases => Set<IntegrationRelease>();
    public DbSet<IntegrationReleaseApp> IntegrationReleaseApps => Set<IntegrationReleaseApp>();
    public DbSet<PipelineTemplate> PipelineTemplates => Set<PipelineTemplate>();
    public DbSet<PipelineTemplateStage> PipelineTemplateStages => Set<PipelineTemplateStage>();
    public DbSet<PipelineTemplateJob> PipelineTemplateJobs => Set<PipelineTemplateJob>();
    public DbSet<Pipeline> Pipelines => Set<Pipeline>();
    public DbSet<PipelineJob> PipelineJobs => Set<PipelineJob>();
    public DbSet<Deploy> Deploys => Set<Deploy>();
    public DbSet<Monitor> Monitors => Set<Monitor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MetaServerDbContext).Assembly);
    }
}
```

## 8. 创建配置类

### Step 8.1 创建 14 个配置文件

创建这些文件：

```text
src/Infrastructure/Persistence/Configurations/ApplicationConfiguration.cs
src/Infrastructure/Persistence/Configurations/SubApplicationConfiguration.cs
src/Infrastructure/Persistence/Configurations/UserConfiguration.cs
src/Infrastructure/Persistence/Configurations/RequirementConfiguration.cs
src/Infrastructure/Persistence/Configurations/IterationConfiguration.cs
src/Infrastructure/Persistence/Configurations/IntegrationReleaseConfiguration.cs
src/Infrastructure/Persistence/Configurations/IntegrationReleaseAppConfiguration.cs
src/Infrastructure/Persistence/Configurations/Pipelines/Templates/PipelineTemplateConfiguration.cs
src/Infrastructure/Persistence/Configurations/Pipelines/Templates/PipelineTemplateStageConfiguration.cs
src/Infrastructure/Persistence/Configurations/Pipelines/Templates/PipelineTemplateJobConfiguration.cs
src/Infrastructure/Persistence/Configurations/Pipelines/PipelineConfiguration.cs
src/Infrastructure/Persistence/Configurations/Pipelines/PipelineJobConfiguration.cs
src/Infrastructure/Persistence/Configurations/DeployConfiguration.cs
src/Infrastructure/Persistence/Configurations/MonitorConfiguration.cs
```

每个配置类都实现：

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
```

### Step 8.2 配置标准字段

普通 int 主键：

```csharp
builder.HasKey(entity => entity.Id);

builder.Property(entity => entity.Id)
    .HasColumnName("id")
    .ValueGeneratedOnAdd();
```

Guid 主键：

```csharp
builder.HasKey(entity => entity.Id);

builder.Property(entity => entity.Id)
    .HasColumnName("id")
    .HasColumnType("uuid")
    .ValueGeneratedOnAdd();
```

时间字段：

```csharp
builder.Property(entity => entity.CreatedAt)
    .HasColumnName("created_at")
    .HasColumnType("timestamp with time zone")
    .IsRequired();
```

JSON 字段：

```csharp
builder.Property(entity => entity.Extra)
    .HasColumnName("extra")
    .HasColumnType("jsonb")
    .IsRequired(false);
```

### Step 8.3 配置 Application

文件：

```text
src/Infrastructure/Persistence/Configurations/ApplicationConfiguration.cs
```

关键配置：

```csharp
using Domain.Entities;
using Domain.Entities.Pipelines.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("applications");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.AppKey).HasColumnName("app_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.ProjectType).HasColumnName("project_type").HasMaxLength(32).HasDefaultValue("fe").IsRequired();
        builder.Property(entity => entity.DeployKey).HasColumnName("deploy_key").HasMaxLength(128);
        builder.Property(entity => entity.GitId).HasColumnName("git_id");
        builder.Property(entity => entity.RegistryKey).HasColumnName("registry_key").HasMaxLength(64).HasDefaultValue("fe").IsRequired();
        builder.Property(entity => entity.GitName).HasColumnName("git_name").HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.GitRepo).HasColumnName("git_repo").HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.MainBranch).HasColumnName("main_branch").HasMaxLength(128).HasDefaultValue("main").IsRequired();
        builder.Property(entity => entity.PreBranch).HasColumnName("pre_branch").HasMaxLength(128).HasDefaultValue("pre").IsRequired();
        builder.Property(entity => entity.StageBranch).HasColumnName("stage_branch").HasMaxLength(128).HasDefaultValue("stage").IsRequired();
        builder.Property(entity => entity.DevBranch).HasColumnName("dev_branch").HasMaxLength(128).HasDefaultValue("dev").IsRequired();
        builder.Property(entity => entity.GitNamespaceId).HasColumnName("git_namespace_id");
        builder.Property(entity => entity.TriggerToken).HasColumnName("trigger_token").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.OwnerName).HasColumnName("owner_name").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status");
        builder.Property(entity => entity.Remark).HasColumnName("remark").HasMaxLength(1024);
        builder.Property(entity => entity.Ranchers).HasColumnName("ranchers").HasColumnType("jsonb");
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedByUserId).HasColumnName("updated_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.UpdatedByUserName).HasColumnName("updated_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasIndex(entity => entity.AppKey)
            .HasDatabaseName("ix_applications_app_key")
            .IsUnique();

        builder.HasMany(entity => entity.SubApplications)
            .WithOne(entity => entity.ParentApplication)
            .HasForeignKey(entity => entity.ParentApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.PipelineTemplates)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "application_pipeline_templates",
                right => right
                    .HasOne<PipelineTemplate>()
                    .WithMany()
                    .HasForeignKey("pipeline_template_id")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Application>()
                    .WithMany()
                    .HasForeignKey("application_id")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("application_pipeline_templates");
                    join.HasKey("application_id", "pipeline_template_id");
                    join.IndexerProperty<int>("application_id").HasColumnName("application_id");
                    join.IndexerProperty<int>("pipeline_template_id").HasColumnName("pipeline_template_id");
                });
    }
}
```

### Step 8.4 创建其他配置类

下面把剩余 13 个配置类都写完整。每张表都按字段逐项配置 `HasColumnName`；字符串字段补 `HasMaxLength` 和是否必填；PostgreSQL 需要明确的类型，例如 `uuid`、`jsonb`、`text`、`timestamp with time zone`，用 `HasColumnType` 写清楚；有业务默认值的字段用 `HasDefaultValue` 固化在数据库模型里。后面的 Step 8.5、Step 8.6 会再解释关系和索引为什么这样配。

#### `SubApplicationConfiguration.cs`

```csharp
using Domain.Entities;
using Domain.Entities.Pipelines.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class SubApplicationConfiguration : IEntityTypeConfiguration<SubApplication>
{
    public void Configure(EntityTypeBuilder<SubApplication> builder)
    {
        builder.ToTable("sub_applications");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.ParentApplicationId).HasColumnName("parent_application_id");
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.AppKey).HasColumnName("app_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Platform).HasColumnName("platform").HasMaxLength(64);
        builder.Property(entity => entity.DeployKey).HasColumnName("deploy_key").HasMaxLength(128);
        builder.Property(entity => entity.GitId).HasColumnName("git_id");
        builder.Property(entity => entity.RegistryKey).HasColumnName("registry_key").HasMaxLength(64).HasDefaultValue("fe").IsRequired();
        builder.Property(entity => entity.GitName).HasColumnName("git_name").HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.GitRepo).HasColumnName("git_repo").HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.MainBranch).HasColumnName("main_branch").HasMaxLength(128).HasDefaultValue("main").IsRequired();
        builder.Property(entity => entity.PreBranch).HasColumnName("pre_branch").HasMaxLength(128).HasDefaultValue("pre").IsRequired();
        builder.Property(entity => entity.StageBranch).HasColumnName("stage_branch").HasMaxLength(128).HasDefaultValue("stage").IsRequired();
        builder.Property(entity => entity.DevBranch).HasColumnName("dev_branch").HasMaxLength(128).HasDefaultValue("dev").IsRequired();
        builder.Property(entity => entity.ProdSiteAddress).HasColumnName("prod_site_address").HasMaxLength(512).HasDefaultValue(string.Empty).IsRequired();
        builder.Property(entity => entity.PreSiteAddress).HasColumnName("pre_site_address").HasMaxLength(512).HasDefaultValue(string.Empty).IsRequired();
        builder.Property(entity => entity.StageSiteAddress).HasColumnName("stage_site_address").HasMaxLength(512).HasDefaultValue(string.Empty).IsRequired();
        builder.Property(entity => entity.DevSiteAddress).HasColumnName("dev_site_address").HasMaxLength(512).HasDefaultValue(string.Empty).IsRequired();
        builder.Property(entity => entity.GitNamespaceId).HasColumnName("git_namespace_id");
        builder.Property(entity => entity.TriggerToken).HasColumnName("trigger_token").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Remark).HasColumnName("remark").HasMaxLength(1024);
        builder.Property(entity => entity.PublicPath).HasColumnName("public_path").HasMaxLength(512);
        builder.Property(entity => entity.UploadToOss).HasColumnName("upload_to_oss").HasDefaultValue(false);
        builder.Property(entity => entity.AppType).HasColumnName("app_type").HasMaxLength(32).HasDefaultValue("saas").IsRequired();
        builder.Property(entity => entity.Variables).HasColumnName("variables").HasColumnType("jsonb");
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedByUserId).HasColumnName("updated_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.UpdatedByUserName).HasColumnName("updated_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasIndex(entity => entity.AppKey)
            .HasDatabaseName("ix_sub_applications_app_key")
            .IsUnique();

        builder.HasMany(entity => entity.PipelineTemplates)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "sub_application_pipeline_templates",
                right => right
                    .HasOne<PipelineTemplate>()
                    .WithMany()
                    .HasForeignKey("pipeline_template_id")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<SubApplication>()
                    .WithMany()
                    .HasForeignKey("sub_application_id")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("sub_application_pipeline_templates");
                    join.HasKey("sub_application_id", "pipeline_template_id");
                    join.IndexerProperty<int>("sub_application_id").HasColumnName("sub_application_id");
                    join.IndexerProperty<int>("pipeline_template_id").HasColumnName("pipeline_template_id");
                });
    }
}
```

#### `UserConfiguration.cs`

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.UserId).HasColumnName("user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.DingTalkUserId).HasColumnName("ding_talk_user_id").HasMaxLength(64);
        builder.Property(entity => entity.ManagerUserId).HasColumnName("manager_user_id").HasMaxLength(64);
        builder.Property(entity => entity.ManagerDingTalkUserId).HasColumnName("manager_ding_talk_user_id").HasMaxLength(64);
        builder.Property(entity => entity.Email).HasColumnName("email").HasMaxLength(128);
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.RealName).HasColumnName("real_name").HasMaxLength(64);
        builder.Property(entity => entity.Mobile).HasColumnName("mobile").HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Role).HasColumnName("role");
        builder.Property(entity => entity.Status).HasColumnName("status");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasIndex(entity => entity.Mobile)
            .HasDatabaseName("ix_users_mobile")
            .IsUnique();

        builder.HasIndex(entity => entity.DingTalkUserId)
            .HasDatabaseName("ix_users_ding_talk_user_id");
    }
}
```

#### `RequirementConfiguration.cs`

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class RequirementConfiguration : IEntityTypeConfiguration<Requirement>
{
    public void Configure(EntityTypeBuilder<Requirement> builder)
    {
        builder.ToTable("requirements");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status");
        builder.Property(entity => entity.DocumentUrl).HasColumnName("document_url").HasMaxLength(1024).IsRequired();
        builder.Property(entity => entity.Priority).HasColumnName("priority");
        builder.Property(entity => entity.Remark).HasColumnName("remark").HasMaxLength(2048);
        builder.Property(entity => entity.OnlineAt).HasColumnName("online_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.SubmittedTestAt).HasColumnName("submitted_test_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedByUserId).HasColumnName("updated_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.UpdatedByUserName).HasColumnName("updated_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasMany(entity => entity.Developers)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "requirement_developers",
                right => right
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey("user_id")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Requirement>()
                    .WithMany()
                    .HasForeignKey("requirement_id")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("requirement_developers");
                    join.HasKey("requirement_id", "user_id");
                    join.IndexerProperty<int>("requirement_id").HasColumnName("requirement_id");
                    join.IndexerProperty<int>("user_id").HasColumnName("user_id");
                });

        builder.HasMany(entity => entity.Followers)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "requirement_followers",
                right => right
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey("user_id")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Requirement>()
                    .WithMany()
                    .HasForeignKey("requirement_id")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("requirement_followers");
                    join.HasKey("requirement_id", "user_id");
                    join.IndexerProperty<int>("requirement_id").HasColumnName("requirement_id");
                    join.IndexerProperty<int>("user_id").HasColumnName("user_id");
                });
    }
}
```

#### `IterationConfiguration.cs`

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class IterationConfiguration : IEntityTypeConfiguration<Iteration>
{
    public void Configure(EntityTypeBuilder<Iteration> builder)
    {
        builder.ToTable("iterations");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.ApplicationId).HasColumnName("application_id");
        builder.Property(entity => entity.SubApplicationId).HasColumnName("sub_application_id");
        builder.Property(entity => entity.IntegrationReleaseId).HasColumnName("integration_release_id");
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(512).IsRequired();
        builder.Property(entity => entity.ApplicationName).HasColumnName("application_name").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.SubApplicationName).HasColumnName("sub_application_name").HasMaxLength(128);
        builder.Property(entity => entity.Branch).HasColumnName("branch").HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.OriginalCommit).HasColumnName("original_commit").HasMaxLength(64);
        builder.Property(entity => entity.Status).HasColumnName("status");
        builder.Property(entity => entity.Remark).HasColumnName("remark").HasMaxLength(2048);
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedByUserId).HasColumnName("updated_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.UpdatedByUserName).HasColumnName("updated_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasOne(entity => entity.Application)
            .WithMany()
            .HasForeignKey(entity => entity.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.SubApplication)
            .WithMany()
            .HasForeignKey(entity => entity.SubApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.IntegrationRelease)
            .WithMany()
            .HasForeignKey(entity => entity.IntegrationReleaseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(entity => entity.Requirements)
            .WithMany(entity => entity.Iterations)
            .UsingEntity<Dictionary<string, object>>(
                "iteration_requirements",
                right => right
                    .HasOne<Requirement>()
                    .WithMany()
                    .HasForeignKey("requirement_id")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Iteration>()
                    .WithMany()
                    .HasForeignKey("iteration_id")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("iteration_requirements");
                    join.HasKey("iteration_id", "requirement_id");
                    join.IndexerProperty<int>("iteration_id").HasColumnName("iteration_id");
                    join.IndexerProperty<int>("requirement_id").HasColumnName("requirement_id");
                });
    }
}
```

#### `IntegrationReleaseConfiguration.cs`

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class IntegrationReleaseConfiguration : IEntityTypeConfiguration<IntegrationRelease>
{
    public void Configure(EntityTypeBuilder<IntegrationRelease> builder)
    {
        builder.ToTable("integration_releases");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.Version).HasColumnName("version").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(512);
        builder.Property(entity => entity.Branch).HasColumnName("branch").HasMaxLength(256);
        builder.Property(entity => entity.Remark).HasColumnName("remark").HasMaxLength(2048);
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasIndex(entity => entity.Version)
            .HasDatabaseName("ix_integration_releases_version")
            .IsUnique();

        builder.HasMany(entity => entity.ReleaseApps)
            .WithOne(entity => entity.IntegrationRelease)
            .HasForeignKey(entity => entity.IntegrationReleaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### `IntegrationReleaseAppConfiguration.cs`

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class IntegrationReleaseAppConfiguration : IEntityTypeConfiguration<IntegrationReleaseApp>
{
    public void Configure(EntityTypeBuilder<IntegrationReleaseApp> builder)
    {
        builder.ToTable("integration_release_apps");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.IntegrationReleaseId).HasColumnName("integration_release_id");
        builder.Property(entity => entity.ApplicationId).HasColumnName("application_id");
        builder.Property(entity => entity.SubApplicationId).HasColumnName("sub_application_id");
        builder.Property(entity => entity.AppKey).HasColumnName("app_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.ApplicationName).HasColumnName("application_name").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.SubApplicationName).HasColumnName("sub_application_name").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.IterationId).HasColumnName("iteration_id");

        builder.HasOne(entity => entity.Application)
            .WithMany()
            .HasForeignKey(entity => entity.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.SubApplication)
            .WithMany()
            .HasForeignKey(entity => entity.SubApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Iteration)
            .WithMany()
            .HasForeignKey(entity => entity.IterationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

#### `PipelineTemplateConfiguration.cs`

```csharp
using Domain.Entities.Pipelines.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Pipelines.Templates;

public sealed class PipelineTemplateConfiguration : IEntityTypeConfiguration<PipelineTemplate>
{
    public void Configure(EntityTypeBuilder<PipelineTemplate> builder)
    {
        builder.ToTable("pipeline_templates");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.TemplateKey).HasColumnName("template_key").HasMaxLength(128);
        builder.Property(entity => entity.Status).HasColumnName("status").HasDefaultValue(1);
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedByUserId).HasColumnName("updated_by_user_id").HasMaxLength(64);
        builder.Property(entity => entity.UpdatedByUserName).HasColumnName("updated_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

        builder.HasIndex(entity => entity.Name)
            .HasDatabaseName("ix_pipeline_templates_name")
            .IsUnique();

        builder.HasIndex(entity => entity.TemplateKey)
            .HasDatabaseName("ix_pipeline_templates_template_key")
            .IsUnique();

        builder.HasMany(entity => entity.Stages)
            .WithOne(entity => entity.PipelineTemplate)
            .HasForeignKey(entity => entity.PipelineTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### `PipelineTemplateStageConfiguration.cs`

```csharp
using Domain.Entities.Pipelines.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Pipelines.Templates;

public sealed class PipelineTemplateStageConfiguration : IEntityTypeConfiguration<PipelineTemplateStage>
{
    public void Configure(EntityTypeBuilder<PipelineTemplateStage> builder)
    {
        builder.ToTable("pipeline_template_stages");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.PipelineTemplateId).HasColumnName("pipeline_template_id");
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Seq).HasColumnName("seq");

        builder.HasMany(entity => entity.Jobs)
            .WithOne(entity => entity.PipelineTemplateStage)
            .HasForeignKey(entity => entity.PipelineTemplateStageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### `PipelineTemplateJobConfiguration.cs`

```csharp
using Domain.Entities.Pipelines.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Pipelines.Templates;

public sealed class PipelineTemplateJobConfiguration : IEntityTypeConfiguration<PipelineTemplateJob>
{
    public void Configure(EntityTypeBuilder<PipelineTemplateJob> builder)
    {
        builder.ToTable("pipeline_template_jobs");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(entity => entity.PipelineTemplateStageId).HasColumnName("pipeline_template_stage_id");
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.JobKey).HasColumnName("job_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.StageSeq).HasColumnName("stage_seq");
        builder.Property(entity => entity.Extra).HasColumnName("extra").HasColumnType("jsonb");
    }
}
```

#### `PipelineConfiguration.cs`

```csharp
using Domain.Entities.Pipelines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Pipelines;

public sealed class PipelineConfiguration : IEntityTypeConfiguration<Pipeline>
{
    public void Configure(EntityTypeBuilder<Pipeline> builder)
    {
        builder.ToTable("pipelines");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedOnAdd();
        builder.Property(entity => entity.AppKey).HasColumnName("app_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.IterationId).HasColumnName("iteration_id");
        builder.Property(entity => entity.RepoId).HasColumnName("repo_id");
        builder.Property(entity => entity.RegistryKey).HasColumnName("registry_key").HasMaxLength(64).HasDefaultValue("fe").IsRequired();
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.Status).HasColumnName("status");
        builder.Property(entity => entity.StageSeq).HasColumnName("stage_seq").HasDefaultValue(-1);
        builder.Property(entity => entity.PipelineTemplateId).HasColumnName("pipeline_template_id");
        builder.Property(entity => entity.Branch).HasColumnName("branch").HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.Content).HasColumnName("content").HasMaxLength(2048);
        builder.Property(entity => entity.SwimLane).HasColumnName("swim_lane").HasMaxLength(128);
        builder.Property(entity => entity.ForceUpdate).HasColumnName("force_update");
        builder.Property(entity => entity.Extra).HasColumnName("extra").HasColumnType("jsonb");

        builder.HasIndex(entity => entity.IterationId)
            .HasDatabaseName("ix_pipelines_iteration_id");

        builder.HasIndex(entity => entity.AppKey)
            .HasDatabaseName("ix_pipelines_app_key");

        builder.HasOne(entity => entity.PipelineTemplate)
            .WithMany()
            .HasForeignKey(entity => entity.PipelineTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.Jobs)
            .WithOne(entity => entity.Pipeline)
            .HasForeignKey(entity => entity.PipelineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### `PipelineJobConfiguration.cs`

```csharp
using Domain.Entities.Pipelines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations.Pipelines;

public sealed class PipelineJobConfiguration : IEntityTypeConfiguration<PipelineJob>
{
    public void Configure(EntityTypeBuilder<PipelineJob> builder)
    {
        builder.ToTable("pipeline_jobs");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedOnAdd();
        builder.Property(entity => entity.PipelineId).HasColumnName("pipeline_id").HasColumnType("uuid");
        builder.Property(entity => entity.StageSeq).HasColumnName("stage_seq");
        builder.Property(entity => entity.JobKey).HasColumnName("job_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status");
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.UnitKey).HasColumnName("unit_key").HasMaxLength(256);
        builder.Property(entity => entity.Extra).HasColumnName("extra").HasColumnType("jsonb");

        builder.HasIndex(entity => new
            {
                entity.PipelineId,
                entity.StageSeq,
                entity.JobKey,
            })
            .HasDatabaseName("ix_pipeline_jobs_pipeline_stage_job")
            .IsUnique();
    }
}
```

#### `DeployConfiguration.cs`

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class DeployConfiguration : IEntityTypeConfiguration<Deploy>
{
    public void Configure(EntityTypeBuilder<Deploy> builder)
    {
        builder.ToTable("deploys");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedOnAdd();
        builder.Property(entity => entity.PipelineId).HasColumnName("pipeline_id").HasColumnType("uuid");
        builder.Property(entity => entity.AppKey).HasColumnName("app_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.IterationId).HasColumnName("iteration_id");
        builder.Property(entity => entity.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedByUserName).HasColumnName("created_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.Env).HasColumnName("env").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Version).HasColumnName("version").HasMaxLength(256);
        builder.Property(entity => entity.UseVpn).HasColumnName("use_vpn");
        builder.Property(entity => entity.DeployType).HasColumnName("deploy_type");
        builder.Property(entity => entity.SwimLane).HasColumnName("swim_lane").HasMaxLength(128);
        builder.Property(entity => entity.IntegrationReleaseVersion).HasColumnName("integration_release_version").HasMaxLength(128);

        builder.HasIndex(entity => entity.AppKey)
            .HasDatabaseName("ix_deploys_app_key");

        builder.HasIndex(entity => entity.PipelineId)
            .HasDatabaseName("ix_deploys_pipeline_id");

        builder.HasOne(entity => entity.Pipeline)
            .WithMany()
            .HasForeignKey(entity => entity.PipelineId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
```

#### `MonitorConfiguration.cs`

```csharp
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class MonitorConfiguration : IEntityTypeConfiguration<Monitor>
{
    public void Configure(EntityTypeBuilder<Monitor> builder)
    {
        builder.ToTable("monitors");
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedOnAdd();
        builder.Property(entity => entity.AppKey).HasColumnName("app_key").HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Env).HasColumnName("env").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Version).HasColumnName("version").HasMaxLength(256);
        builder.Property(entity => entity.SourceUuid).HasColumnName("source_uuid").HasMaxLength(64);
        builder.Property(entity => entity.TenantId).HasColumnName("tenant_id").HasMaxLength(64);
        builder.Property(entity => entity.TenantName).HasColumnName("tenant_name").HasMaxLength(128);
        builder.Property(entity => entity.UserId).HasColumnName("user_id").HasMaxLength(64);
        builder.Property(entity => entity.UserName).HasColumnName("user_name").HasMaxLength(64);
        builder.Property(entity => entity.Url).HasColumnName("url").HasMaxLength(2048);
        builder.Property(entity => entity.Browser).HasColumnName("browser").HasMaxLength(512);
        builder.Property(entity => entity.Message).HasColumnName("message").HasColumnType("text");
        builder.Property(entity => entity.Stack).HasColumnName("stack").HasColumnType("text");
        builder.Property(entity => entity.Status).HasColumnName("status");
        builder.Property(entity => entity.Remark).HasColumnName("remark").HasMaxLength(2048);
        builder.Property(entity => entity.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.ResolvedByUserId).HasColumnName("resolved_by_user_id").HasMaxLength(64);
        builder.Property(entity => entity.ResolvedByUserName).HasColumnName("resolved_by_user_name").HasMaxLength(64);
        builder.Property(entity => entity.ResolvedAt).HasColumnName("resolved_at").HasColumnType("timestamp with time zone");

        builder.HasIndex(entity => new
            {
                entity.AppKey,
                entity.Env,
            })
            .HasDatabaseName("ix_monitors_app_key_env");

        builder.HasIndex(entity => entity.Status)
            .HasDatabaseName("ix_monitors_status");
    }
}
```

### Step 8.5 配置关系

需要配置这些关系：

```text
applications 1 - N sub_applications
applications N - N pipeline_templates，应用侧选择可用模板
sub_applications N - N pipeline_templates，子应用侧选择可用模板
pipeline_templates 1 - N pipeline_template_stages
pipeline_template_stages 1 - N pipeline_template_jobs
requirements N - N users，开发人员关系
requirements N - N users，关注人员关系
iterations N - N requirements
integration_releases 1 - N integration_release_apps
pipelines 1 - N pipeline_jobs
```

建议中间表命名：

```text
application_pipeline_templates
sub_application_pipeline_templates
requirement_developers
requirement_followers
iteration_requirements
```

多对多示例：

```csharp
builder.HasMany(entity => entity.PipelineTemplates)
    .WithMany()
    .UsingEntity("application_pipeline_templates");
```

子应用关联模板同理放在 `SubApplicationConfiguration`：

```csharp
builder.HasMany(entity => entity.PipelineTemplates)
    .WithMany()
    .UsingEntity("sub_application_pipeline_templates");
```

这里故意不在 `PipelineTemplate` 上放 `Applications` 或 `SubApplications`。模板是独立配置；应用知道自己能用哪些模板就够了。以后如果要做“模板被哪些应用使用”的页面，用查询服务从中间表查，不要为了一个反向查询把模板实体和应用实体互相绑死。

如果中间关系以后需要自己的字段，例如 `created_at`、`created_by_user_id`、排序号或状态，就不要用匿名中间表，改成显式 join entity。今天这些关联暂时没有独立业务字段，所以使用 EF Core skip navigation。

`Requirement` 到 `User` 的两组关系建议做成需求侧单向导航。这样 `Requirement` 能表达开发人员和关注人员，`User` 不需要反向持有需求集合：

```csharp
builder.HasMany(entity => entity.Developers)
    .WithMany()
    .UsingEntity("requirement_developers");

builder.HasMany(entity => entity.Followers)
    .WithMany()
    .UsingEntity("requirement_followers");
```

如果要把中间表列名也显式写清楚，使用这个版本：

```csharp
builder.HasMany(entity => entity.Developers)
    .WithMany()
    .UsingEntity<Dictionary<string, object>>(
        "requirement_developers",
        right => right
            .HasOne<User>()
            .WithMany()
            .HasForeignKey("user_id")
            .OnDelete(DeleteBehavior.Cascade),
        left => left
            .HasOne<Requirement>()
            .WithMany()
            .HasForeignKey("requirement_id")
            .OnDelete(DeleteBehavior.Cascade));
```

一对多示例：

```csharp
builder.HasMany(entity => entity.Stages)
    .WithOne(entity => entity.PipelineTemplate)
    .HasForeignKey(entity => entity.PipelineTemplateId)
    .OnDelete(DeleteBehavior.Cascade);
```

`PipelineConfiguration` 中配置流水线和任务。`PipelineJob` 是流水线执行的一部分，可以随流水线级联删除：

```csharp
builder.HasMany(entity => entity.Jobs)
    .WithOne(entity => entity.Pipeline)
    .HasForeignKey(entity => entity.PipelineId)
    .OnDelete(DeleteBehavior.Cascade);
```

`DeployConfiguration` 中配置发布记录和流水线。`Deploy` 是审计记录，删除流水线时不要删除发布记录：

```csharp
builder.HasOne(entity => entity.Pipeline)
    .WithMany()
    .HasForeignKey(entity => entity.PipelineId)
    .OnDelete(DeleteBehavior.SetNull);
```

`IterationConfiguration` 中配置迭代和应用。迭代依附主应用，子应用和集成发布可以为空：

```csharp
builder.HasOne(entity => entity.Application)
    .WithMany()
    .HasForeignKey(entity => entity.ApplicationId)
    .OnDelete(DeleteBehavior.Restrict);

builder.HasOne(entity => entity.SubApplication)
    .WithMany()
    .HasForeignKey(entity => entity.SubApplicationId)
    .OnDelete(DeleteBehavior.Restrict);

builder.HasOne(entity => entity.IntegrationRelease)
    .WithMany()
    .HasForeignKey(entity => entity.IntegrationReleaseId)
    .OnDelete(DeleteBehavior.SetNull);
```

删除行为建议：

- `PipelineTemplate -> Stage -> Job` 使用 `Cascade`，模板内部是强聚合。
- `Application -> SubApplication` 使用 `Restrict`，避免误删应用时连带删除子应用。
- `IntegrationRelease -> IntegrationReleaseApp` 使用 `Cascade`。
- `Pipeline -> PipelineJob` 使用 `Cascade`。
- `Pipeline -> Deploy` 使用 `SetNull`，保留发布审计记录。

### Step 8.6 配置关键索引

至少配置这些索引：

```text
users:
  ix_users_mobile unique(mobile)
  ix_users_ding_talk_user_id(ding_talk_user_id)

sub_applications:
  ix_sub_applications_app_key unique(app_key)

integration_releases:
  ix_integration_releases_version unique(version)

pipeline_templates:
  ix_pipeline_templates_name unique(name)
  ix_pipeline_templates_template_key unique(template_key)

pipelines:
  ix_pipelines_iteration_id(iteration_id)
  ix_pipelines_app_key(app_key)

pipeline_jobs:
  ix_pipeline_jobs_pipeline_stage_job unique(pipeline_id, stage_seq, job_key)

deploys:
  ix_deploys_app_key(app_key)
  ix_deploys_pipeline_id(pipeline_id)

monitors:
  ix_monitors_app_key_env(app_key, env)
  ix_monitors_status(status)
```

## 9. 注册 DbContext

### Step 9.1 创建注册扩展

创建文件：

```text
src/Infrastructure/Persistence/PersistenceRegistration.cs
```

填入：

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

public static class PersistenceRegistration
{
    public static IServiceCollection AddMetaServerPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("Postgres")["ConnectionString"];

        services.AddDbContext<MetaServerDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        return services;
    }
}
```

### Step 9.2 在 `Program.cs` 注册

打开：

```text
src/Api/Program.cs
```

顶部加入：

```csharp
using Infrastructure.Persistence;
```

在配置注册附近加入：

```csharp
builder.Services.AddMetaServerPersistence(builder.Configuration);
```

建议放在：

```csharp
builder.Services.AddApplicationOptions();
builder.Services.AddMetaServerPersistence(builder.Configuration);
```

## 10. 删除模板空类

### Step 10.1 删除 `Class1.cs`

执行：

```bash
rm -f src/Domain/Class1.cs src/Application/Class1.cs src/Infrastructure/Class1.cs
```

确认：

```bash
find src -name Class1.cs
```

如果没有输出，说明删干净了。

## 11. 编写 metadata 测试

测试放在最后，是为了先把 EF Core 的实体、关系和配置学完整，再用测试反向确认自己有没有建对。今天的 metadata 测试不连真实数据库，只检查 EF Core 内存里的模型定义。

### Step 11.1 让 UnitTests 引用 Infrastructure

执行：

```bash
dotnet add tests/UnitTests/UnitTests.csproj reference src/Infrastructure/Infrastructure.csproj
```

metadata 测试会直接创建 `MetaServerDbContext`，所以测试项目要能访问 `Infrastructure`。

### Step 11.2 创建测试目录

执行：

```bash
mkdir -p tests/UnitTests/Persistence
```

### Step 11.3 创建测试文件

创建文件：

```text
tests/UnitTests/Persistence/MetaServerDbContextMetadataTests.cs
```

填入：

```csharp
using Domain.Entities;
using Domain.Entities.Pipelines;
using Domain.Entities.Pipelines.Templates;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Persistence;

public sealed class MetaServerDbContextMetadataTests
{
    private readonly MetaServerDbContext _dbContext = new(
        new DbContextOptionsBuilder<MetaServerDbContext>()
            .UseNpgsql("Host=localhost;Database=metadata_test;Username=test;Password=test")
            .Options);

    [Fact]
    public void Model_contains_day03_core_tables()
    {
        var tableNames = _dbContext.Model
            .GetEntityTypes()
            .Select(type => type.GetTableName())
            .Where(tableName => tableName is not null)
            .ToHashSet();

        string[] expectedTables =
        [
            "applications",
            "sub_applications",
            "users",
            "requirements",
            "iterations",
            "integration_releases",
            "integration_release_apps",
            "pipeline_templates",
            "pipeline_template_stages",
            "pipeline_template_jobs",
            "pipelines",
            "pipeline_jobs",
            "deploys",
            "monitors",
        ];

        Assert.All(expectedTables, tableName => Assert.Contains(tableName, tableNames));
    }

    [Theory]
    [InlineData(typeof(Application), "applications", typeof(int))]
    [InlineData(typeof(SubApplication), "sub_applications", typeof(int))]
    [InlineData(typeof(User), "users", typeof(int))]
    [InlineData(typeof(Requirement), "requirements", typeof(int))]
    [InlineData(typeof(Iteration), "iterations", typeof(int))]
    [InlineData(typeof(IntegrationRelease), "integration_releases", typeof(int))]
    [InlineData(typeof(IntegrationReleaseApp), "integration_release_apps", typeof(int))]
    [InlineData(typeof(PipelineTemplate), "pipeline_templates", typeof(int))]
    [InlineData(typeof(PipelineTemplateStage), "pipeline_template_stages", typeof(int))]
    [InlineData(typeof(PipelineTemplateJob), "pipeline_template_jobs", typeof(int))]
    [InlineData(typeof(Pipeline), "pipelines", typeof(Guid))]
    [InlineData(typeof(PipelineJob), "pipeline_jobs", typeof(Guid))]
    [InlineData(typeof(Deploy), "deploys", typeof(Guid))]
    [InlineData(typeof(Monitor), "monitors", typeof(Guid))]
    public void Primary_keys_are_configured(Type entityType, string tableName, Type keyType)
    {
        var entity = _dbContext.Model.FindEntityType(entityType);

        Assert.NotNull(entity);
        Assert.Equal(tableName, entity.GetTableName());

        var key = Assert.Single(entity.FindPrimaryKey()!.Properties);
        Assert.Equal("id", key.GetColumnName());
        Assert.Equal(keyType, key.ClrType);
    }

    [Theory]
    [InlineData(typeof(Application), "app_key")]
    [InlineData(typeof(Application), "created_at")]
    [InlineData(typeof(SubApplication), "parent_application_id")]
    [InlineData(typeof(SubApplication), "upload_to_oss")]
    [InlineData(typeof(Iteration), "integration_release_id")]
    [InlineData(typeof(IntegrationReleaseApp), "sub_application_id")]
    [InlineData(typeof(Pipeline), "pipeline_template_id")]
    [InlineData(typeof(PipelineJob), "pipeline_id")]
    [InlineData(typeof(Deploy), "integration_release_version")]
    [InlineData(typeof(Monitor), "resolved_at")]
    public void Important_columns_use_snake_case(Type entityType, string columnName)
    {
        var entity = _dbContext.Model.FindEntityType(entityType);

        Assert.NotNull(entity);
        Assert.Contains(entity.GetProperties(), property => property.GetColumnName() == columnName);
    }

    [Theory]
    [InlineData(typeof(Application), "ranchers")]
    [InlineData(typeof(SubApplication), "variables")]
    [InlineData(typeof(PipelineTemplateJob), "extra")]
    [InlineData(typeof(Pipeline), "extra")]
    [InlineData(typeof(PipelineJob), "extra")]
    public void Json_fields_use_jsonb(Type entityType, string columnName)
    {
        var entity = _dbContext.Model.FindEntityType(entityType);
        var property = entity!.GetProperties().Single(item => item.GetColumnName() == columnName);

        Assert.Equal("jsonb", property.GetColumnType());
    }

    [Theory]
    [InlineData(typeof(Application), nameof(Application.CreatedAt))]
    [InlineData(typeof(Application), nameof(Application.UpdatedAt))]
    [InlineData(typeof(Requirement), nameof(Requirement.OnlineAt))]
    [InlineData(typeof(Pipeline), nameof(Pipeline.CreatedAt))]
    [InlineData(typeof(Deploy), nameof(Deploy.CreatedAt))]
    [InlineData(typeof(Monitor), nameof(Monitor.ResolvedAt))]
    public void Time_fields_use_timestamp_with_time_zone(Type entityType, string propertyName)
    {
        var entity = _dbContext.Model.FindEntityType(entityType);
        var property = entity!.FindProperty(propertyName);

        Assert.NotNull(property);
        Assert.Equal("timestamp with time zone", property.GetColumnType());
    }

    [Fact]
    public void Important_indexes_are_configured()
    {
        AssertHasIndex<User>("ix_users_mobile", true, "mobile");
        AssertHasIndex<User>("ix_users_ding_talk_user_id", false, "ding_talk_user_id");
        AssertHasIndex<SubApplication>("ix_sub_applications_app_key", true, "app_key");
        AssertHasIndex<IntegrationRelease>("ix_integration_releases_version", true, "version");
        AssertHasIndex<PipelineTemplate>("ix_pipeline_templates_name", true, "name");
        AssertHasIndex<PipelineTemplate>("ix_pipeline_templates_template_key", true, "template_key");
        AssertHasIndex<Pipeline>("ix_pipelines_iteration_id", false, "iteration_id");
        AssertHasIndex<PipelineJob>("ix_pipeline_jobs_pipeline_stage_job", true, "pipeline_id", "stage_seq", "job_key");
        AssertHasIndex<Deploy>("ix_deploys_app_key", false, "app_key");
        AssertHasIndex<Monitor>("ix_monitors_app_key_env", false, "app_key", "env");
    }

    [Fact]
    public void Core_relationship_delete_behaviors_are_explicit()
    {
        AssertHasForeignKey<SubApplication>("parent_application_id", DeleteBehavior.Restrict);
        AssertHasForeignKey<PipelineTemplateStage>("pipeline_template_id", DeleteBehavior.Cascade);
        AssertHasForeignKey<PipelineTemplateJob>("pipeline_template_stage_id", DeleteBehavior.Cascade);
        AssertHasForeignKey<PipelineJob>("pipeline_id", DeleteBehavior.Cascade);
        AssertHasForeignKey<Deploy>("pipeline_id", DeleteBehavior.SetNull);
    }

    private void AssertHasIndex<TEntity>(
        string databaseName,
        bool unique,
        params string[] columns)
    {
        var entity = _dbContext.Model.FindEntityType(typeof(TEntity));
        Assert.NotNull(entity);

        var matchedIndex = entity
            .GetIndexes()
            .SingleOrDefault(index =>
                index.GetDatabaseName() == databaseName
                && index.IsUnique == unique
                && index.Properties.Select(property => property.GetColumnName()).SequenceEqual(columns));

        Assert.NotNull(matchedIndex);
    }

    private void AssertHasForeignKey<TEntity>(string columnName, DeleteBehavior deleteBehavior)
    {
        var entity = _dbContext.Model.FindEntityType(typeof(TEntity));
        Assert.NotNull(entity);

        var matchedForeignKey = entity
            .GetForeignKeys()
            .SingleOrDefault(foreignKey =>
                foreignKey.DeleteBehavior == deleteBehavior
                && foreignKey.Properties.Select(property => property.GetColumnName()).SequenceEqual([columnName]));

        Assert.NotNull(matchedForeignKey);
    }
}
```

## 12. 运行测试并修正

### Step 12.1 只跑 metadata 测试

执行：

```bash
dotnet test tests/UnitTests/UnitTests.csproj --filter MetaServerDbContextMetadataTests
```

常见失败：

| 失败 | 原因 | 修法 |
| --- | --- | --- |
| 找不到实体 | namespace 或 DbSet 没写 | 检查实体 namespace 和 `MetaServerDbContext` |
| 表名不对 | `ToTable(...)` 不一致 | 改成文档里的复数表名 |
| 列名不对 | 漏了 `HasColumnName(...)` | 补 `snake_case` 列名 |
| JSON 类型不对 | 漏了 `HasColumnType("jsonb")` | 补 JSON 字段配置 |
| 索引名不对 | `HasDatabaseName(...)` 不一致 | 统一测试和配置 |
| 关系缺失 | 导航属性或 `HasMany/WithOne` 没配 | 补关系配置 |

### Step 12.2 跑全部测试

执行：

```bash
dotnet test
```

验收：

- Day 01、Day 02 测试继续通过。
- Day 03 metadata 测试通过。

### Step 12.3 build

执行：

```bash
dotnet build
```

验收：

- `Build succeeded.`

## 13. 人工复核是否支撑后续功能

### Step 13.1 对照原业务代码检查对象是否遗漏

执行：

```bash
rg -n "@Entity|@Column|@Index|ManyToMany|OneToMany|ManyToOne" /Users/fenghe/workspace/devops/meta-server/src/core/entities
```

检查：

- 应用、子应用、用户、需求、迭代、集成发布、流水线、发布、监控这些业务对象都覆盖了。
- 应用与模板、需求与用户、需求与迭代等关系都能表达。
- `variables`、`extra`、`ranchers` 这些扩展字段保留为 JSON。
- 后续查询常用字段有索引。
- 新字段命名符合 .NET 项目习惯。

### Step 13.2 写学习笔记

创建或追加：

```text
docs/learning-notes/day3.md
```

写入：

```markdown
# Day 3 Notes

## EF Core 建模决策

- 这是全新的 .NET 项目，原 TypeORM 实体只作为业务功能参考。
- 新表名使用复数 snake_case，例如 applications、pipeline_jobs。
- C# 使用 CreatedAt、UpdatedAt、CreatedByUserId、UpdatedByUserId 等 .NET 风格属性。
- PostgreSQL 时间字段使用 timestamp with time zone。
- JSON 扩展字段使用 jsonb。
- 多对多关系使用 EF Core UsingEntity 配置中间表。
- 模板 stage/job 和 pipeline/job 使用级联删除；应用、子应用等业务主数据使用 Restrict。

## 明天风险

- Day 04 生成 migration 后要人工检查 SQL。
- 如果未来要导入旧系统数据，需要单独设计旧表到新表的数据迁移脚本。
```

## 14. 今天完成后的目录应该长这样

```text
dotnet-meta-server/
├── src/
│   ├── Domain/
│   │   └── Entities/
│   │       ├── Application.cs
│   │       ├── Deploy.cs
│   │       ├── IntegrationRelease.cs
│   │       ├── IntegrationReleaseApp.cs
│   │       ├── Iteration.cs
│   │       ├── Monitor.cs
│   │       ├── Requirement.cs
│   │       ├── SubApplication.cs
│   │       ├── User.cs
│   │       └── Pipelines/
│   │           ├── Pipeline.cs
│   │           ├── PipelineJob.cs
│   │           └── Templates/
│   │               ├── PipelineTemplate.cs
│   │               ├── PipelineTemplateJob.cs
│   │               └── PipelineTemplateStage.cs
│   └── Infrastructure/
│       └── Persistence/
│           ├── MetaServerDbContext.cs
│           ├── PersistenceRegistration.cs
│           └── Configurations/
│               ├── ApplicationConfiguration.cs
│               ├── DeployConfiguration.cs
│               ├── IntegrationReleaseConfiguration.cs
│               ├── IntegrationReleaseAppConfiguration.cs
│               ├── IterationConfiguration.cs
│               ├── MonitorConfiguration.cs
│               ├── RequirementConfiguration.cs
│               ├── SubApplicationConfiguration.cs
│               ├── UserConfiguration.cs
│               └── Pipelines/
│                   ├── PipelineConfiguration.cs
│                   ├── PipelineJobConfiguration.cs
│                   └── Templates/
│                       ├── PipelineTemplateConfiguration.cs
│                       ├── PipelineTemplateJobConfiguration.cs
│                       └── PipelineTemplateStageConfiguration.cs
└── tests/
    └── UnitTests/
        └── Persistence/
            └── MetaServerDbContextMetadataTests.cs
```

## 15. 最终验收命令

执行：

```bash
dotnet build
dotnet test
```

验收：

- `dotnet build` 通过。
- `dotnet test` 通过。
- metadata 测试覆盖 14 个核心实体。
- 表名、列名、主键、时间、JSON、索引和关系都符合新 .NET 项目的建模规则。

## 16. 晚上复盘

可以按这个模板写：

```markdown
## 今天学会的 C#/.NET 概念

- DbContext 是 EF Core 访问数据库的入口。
- DbSet<TEntity> 代表一张表的查询入口。
- IEntityTypeConfiguration<TEntity> 负责把 C# 类映射到数据库表结构。
- HasColumnName 控制列名，HasColumnType 控制数据库类型。
- DateTimeOffset 适合表达带时区语义的业务时间。
- jsonb 是 PostgreSQL 原生 JSON 存储类型，适合 variables、extra 这类扩展字段。

## 今天完成的工程产物

- 14 个核心 Entity。
- MetaServerDbContext。
- 14 个实体配置类。
- metadata 测试。

## 明天风险

- migration SQL 要人工检查。
- 旧系统数据迁移不属于 Day 03，后续如果需要再单独设计。
```

## 17. 参考资料

原业务源码参考目录：

```text
/Users/fenghe/workspace/devops/meta-server
```

今天重点参考这些原文件：

```text
/Users/fenghe/workspace/devops/meta-server/src/core/entities/application.entity.ts
/Users/fenghe/workspace/devops/meta-server/src/core/entities/sub.application.entity.ts
/Users/fenghe/workspace/devops/meta-server/src/core/entities/user.entity.ts
/Users/fenghe/workspace/devops/meta-server/src/core/entities/requirement.entity.ts
/Users/fenghe/workspace/devops/meta-server/src/core/entities/iteration.entity.ts
/Users/fenghe/workspace/devops/meta-server/src/core/entities/integration-release.entity.ts
/Users/fenghe/workspace/devops/meta-server/src/core/entities/integration-release-app.entity.ts
/Users/fenghe/workspace/devops/meta-server/src/core/entities/pipeline/template/pipeline.tpl.entity.ts
/Users/fenghe/workspace/devops/meta-server/src/core/entities/pipeline/template/pipeline.tpl.stage.entity.ts
/Users/fenghe/workspace/devops/meta-server/src/core/entities/pipeline/template/pipeline.tpl.job.entity.ts
/Users/fenghe/workspace/devops/meta-server/src/core/entities/pipeline/processor/pipeline.entity.ts
/Users/fenghe/workspace/devops/meta-server/src/core/entities/pipeline/processor/pipeline.job.entity.ts
/Users/fenghe/workspace/devops/meta-server/src/core/entities/deploy.entity.ts
/Users/fenghe/workspace/devops/meta-server/src/core/entities/monitor.entity.ts
```
