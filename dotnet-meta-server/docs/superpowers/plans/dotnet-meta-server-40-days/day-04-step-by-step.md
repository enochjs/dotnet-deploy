# Day 04 Step-by-Step - Migration、种子数据和 Testcontainers

这份文档是 `day-04.md` 的跟做版。Day 04 的目标是把 Day 03 做好的 EF Core 模型，变成可重复创建的数据库结构，并让集成测试不再依赖你本机已经启动好的 PostgreSQL 或 Redis。

今天先记住一个分界：

- Migration 负责数据库结构：有哪些表、列、索引、外键。
- Seed data 负责基础测试数据：有哪些用户、应用、需求、迭代、模板。
- Testcontainers 负责测试环境：测试开始时启动临时 PostgreSQL/Redis，测试结束后自动清理。

原 NestJS 项目只作为功能参考；今天不会迁移旧库数据，也不会追求旧表结构一比一兼容。具体参考文件清单放在文档最后。

## 0. 今天最终要得到什么

完成后，你会在 Day 03 的项目上新增这些能力：

- 本地安装 `dotnet-ef` 工具，项目内可复现 EF Core CLI 版本。
- `Infrastructure` 中有第一版 migration：`InitialCreate`。
- 能生成 migration SQL，并人工检查 PostgreSQL 类型、表名、列名、索引和外键。
- `Infrastructure/Persistence/SeedData` 中有可重复执行的开发/测试种子数据。
- `IntegrationTests` 使用 Testcontainers 启动临时 PostgreSQL。
- `IntegrationTests` 使用 Testcontainers 启动临时 Redis。
- 测试启动时自动执行 `Database.MigrateAsync()`。
- 数据库连接、种子数据、Redis set/get、健康检查集成测试全部通过。

你最后需要确认三件事：

- `dotnet ef migrations list` 能看到 `InitialCreate`。
- `dotnet test` 即使本机没有运行 PostgreSQL/Redis 也能通过。
- 测试输出里能看到 Testcontainers 拉起并清理容器。

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

### Step 1.2 确认 Day 03 代码能编译

执行：

```bash
dotnet build
dotnet test
```

验收：

- `dotnet build` 能看到 `Build succeeded.`
- `dotnet test` 能看到 `Passed!`

如果这里失败，先回到 `day-03-step-by-step.md` 修好实体和配置。今天会生成 migration，如果模型本身不稳定，migration 文件会把错误固化下来。

## 2. 准备 Docker

### Step 2.1 检查 Docker 是否可用

Testcontainers 不会自己提供数据库引擎，它会通过 Docker 启动真正的 PostgreSQL 和 Redis 容器。

执行：

```bash
docker version
```

验收：

- 能看到 Client 和 Server 两部分版本信息。
- 如果只看到 Client，看不到 Server，说明 Docker Desktop 没启动。

再执行：

```bash
docker info
```

验收：

- 命令能正常结束。
- 没有 `Cannot connect to the Docker daemon`。

如果 Docker 没启动，先打开 Docker Desktop，等状态变成 running 后再继续。

## 3. 安装 EF Core CLI 工具

### Step 3.1 创建本地工具清单

执行：

```bash
dotnet new tool-manifest
```

这会创建：

```text
.config/dotnet-tools.json
```

如果提示模板已经存在，说明之前创建过，可以继续下一步。

### Step 3.2 安装 `dotnet-ef`

执行：

```bash
dotnet tool install dotnet-ef --version '10.*'
```

这里使用 `'10.*'`，是为了和当前项目的 `net10.0`、`Microsoft.EntityFrameworkCore 10.x` 保持同一个大版本。EF Core CLI 和运行时包大版本不一致时，容易出现“工具能运行但模型解析异常”的问题。版本号必须加引号：zsh 会把未加引号的 `10.*` 当成文件通配符，直接报 `no matches found`。

验收：

```bash
dotnet tool list
```

你应该看到类似：

```text
Package Id      Version      Commands
dotnet-ef       10.0.*       dotnet-ef
```

### Step 3.3 还原工具

以后别人 clone 这个项目后，不需要猜装什么工具，只要执行：

```bash
dotnet tool restore
```

验收：

- 能看到 `Tool 'dotnet-ef' ... was restored`，或提示工具已经可用。

你可以先这样理解：

- NuGet package 是项目编译和运行需要的依赖。
- .NET local tool 是开发时使用的命令行工具。
- `.config/dotnet-tools.json` 应该提交到 git，这样团队里的 EF CLI 版本一致。

### Step 3.4 给启动项目 Api 添加 Design 包

Day 03 已经把 `Microsoft.EntityFrameworkCore.Design` 加到了 `Infrastructure`。`dotnet add` 默认会给这个包加上 `PrivateAssets=all`，所以它不会传递给引用 `Infrastructure` 的 `Api`。

`dotnet ef` 检查的是 **startup project**，不是 `--project`。今天所有 EF 命令的 `--startup-project` 都是 `Api`，所以 `Api` 必须自己直接引用 Design 包。传递依赖不算。

这是官方约定，不是绕路：Design 包只用于设计期（生成/检查 migration）。`PrivateAssets=all` 可以避免它被发布到生产环境。

执行：

```bash
dotnet add src/Api/Api.csproj package Microsoft.EntityFrameworkCore.Design --version 10.0.11
```

版本写成 `10.0.11`，是为了和 `Infrastructure` 里已有的 EF Core 包对齐。

验收：打开 `src/Api/Api.csproj`，应能看到：

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.11">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

## 4. 确认 EF CLI 能找到 DbContext

### Step 4.1 查看 DbContext

执行：

```bash
dotnet ef dbcontext list \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```

这里有两个 project：

- `--project`：DbContext 和 migration 要放在哪个项目。今天是 `Infrastructure`。
- `--startup-project`：从哪个项目读取运行时配置和依赖注入。今天是 `Api`，因为 `Program.cs` 里注册了 `AddMetaServerPersistence(builder.Configuration)`。

验收：

你应该看到：

```text
Infrastructure.Persistence.MetaServerDbContext
```

### Step 4.2 如果 EF CLI 找不到 DbContext

如果看到：

```text
No DbContext was found
```

先检查 `src/Api/Program.cs` 中有没有这行：

```csharp
builder.Services.AddMetaServerPersistence(builder.Configuration);
```

再检查 `src/Infrastructure/Persistence/MetaServerDbContext.cs` 是否继承了 `DbContext`：

```csharp
public sealed class MetaServerDbContext(DbContextOptions<MetaServerDbContext> options) : DbContext(options)
```

最后重新执行：

```bash
dotnet build
dotnet ef dbcontext list \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```

## 5. 生成第一版 migration

### Step 5.1 创建 migration

执行：

```bash
dotnet ef migrations add InitialCreate \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj \
  --output-dir Persistence/Migrations
```

命令成功后，会生成类似这些文件：

```text
src/Infrastructure/Persistence/Migrations/20260818******_InitialCreate.cs
src/Infrastructure/Persistence/Migrations/20260818******_InitialCreate.Designer.cs
src/Infrastructure/Persistence/Migrations/MetaServerDbContextModelSnapshot.cs
```

你可以先这样理解：

- `*_InitialCreate.cs`：真正的建表和删表逻辑，里面有 `Up()` 和 `Down()`。
- `*.Designer.cs`：EF 生成的模型元数据，通常不手改。
- `ModelSnapshot.cs`：当前模型快照，下次生成 migration 时 EF 会用它判断差异。

### Step 5.2 检查 migration 文件

执行：

```bash
find src/Infrastructure/Persistence/Migrations -type f | sort
```

验收：

- 能看到 3 个 migration 相关文件。
- 文件名包含 `InitialCreate`。

## 6. 人工检查 migration SQL

### Step 6.1 生成 SQL 文件

执行：

```bash
mkdir -p artifacts/migrations
dotnet ef migrations script \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj \
  --output artifacts/migrations/initial-create.sql
```

验收：

```bash
wc -l artifacts/migrations/initial-create.sql
```

你应该看到这个 SQL 文件不是空文件。

### Step 6.2 检查表名

执行：

```bash
rg 'CREATE TABLE' artifacts/migrations/initial-create.sql
```

重点确认这些表存在：

```text
applications
sub_applications
users
requirements
iterations
integration_releases
integration_release_apps
pipeline_templates
pipeline_template_stages
pipeline_template_jobs
pipelines
pipeline_jobs
deploys
app_monitors
```

还会有一些 EF 自动生成的中间表，例如：

```text
application_pipeline_template
sub_application_pipeline_template
requirement_developers
requirement_followers
iteration_requirements
```

这些中间表是多对多关系需要的，不是错误。

### Step 6.3 检查 PostgreSQL 类型

执行：

```bash
rg 'uuid|jsonb|timestamp with time zone|GENERATED BY DEFAULT AS IDENTITY' artifacts/migrations/initial-create.sql
```

本机如果没有 `rg`，会报 `command not found`。改用：

```bash
grep -E 'uuid|jsonb|timestamp with time zone|GENERATED BY DEFAULT AS IDENTITY' artifacts/migrations/initial-create.sql
```

验收重点：

- `pipelines.id`、`pipeline_jobs.id`、`deploys.id`、`app_monitors.id` 使用 `uuid`。
- `applications.ranchers`、`sub_applications.variables`、`pipeline_template_jobs.extra`、`pipelines.extra`、`pipeline_jobs.extra` 使用 `jsonb`。
- `created_at`、`updated_at`、`online_at`、`submitted_test_at`、`resolved_at` 使用 `timestamp with time zone`。
- 普通 `int` 主键使用 identity。

### Step 6.4 检查索引和外键

执行：

```bash
rg 'CREATE (UNIQUE )?INDEX|FOREIGN KEY' artifacts/migrations/initial-create.sql
```

没有 `rg` 时：

```bash
grep -E 'CREATE (UNIQUE )?INDEX|FOREIGN KEY' artifacts/migrations/initial-create.sql
```

验收重点：

- `ix_users_mobile` 是唯一索引。
- `ix_sub_applications_app_key` 是唯一索引。
- `ix_pipeline_jobs_pipeline_stage_job` 是组合唯一索引。
- `sub_applications.parent_application_id` 有外键。
- `pipeline_template_stages.pipeline_template_id` 有外键。
- `pipeline_template_jobs.pipeline_template_stage_id` 有外键。
- `pipeline_jobs.pipeline_id` 有外键。
- `deploys.pipeline_id` 删除行为是 `SET NULL`。

### Step 6.5 如果 migration 里出现奇怪的默认时间

如果配置里写了：

```csharp
.HasDefaultValue(DateTimeOffset.UtcNow)
```

或者 snapshot / SQL 里出现类似：

```csharp
.HasDefaultValue(new DateTimeOffset(new DateTime(2026, 8, 18, 3, 20, 54, 736, DateTimeKind.Unspecified), TimeSpan.Zero))
```

这不是“每次插入取当前时间”。`HasDefaultValue(...)` 会在**建模那一刻**把当时的时间固化成常量。下一次再构建模型时，`DateTimeOffset.UtcNow` 又是一个新值，模型和 snapshot 对不上。

EF Core 9/10 会把这件事升级成错误。`dotnet ef database update` 时会看到：

```text
PendingModelChangesWarning: The model for context 'MetaServerDbContext' changes each time it is built.
This is usually caused by dynamic values used in a 'HasData' call (e.g. `new DateTime()`, `Guid.NewGuid()`).
```

报错文案会提到 `HasData`，但这里真正的原因常常是 `HasDefaultValue(DateTimeOffset.UtcNow)`。

更推荐两种方式：

```csharp
builder.Property(entity => entity.CreatedAt)
    .HasColumnName("created_at")
    .HasColumnType("timestamp with time zone");
```

或者数据库默认值：

```csharp
builder.Property(entity => entity.CreatedAt)
    .HasColumnName("created_at")
    .HasColumnType("timestamp with time zone")
    .HasDefaultValueSql("now()");
```

初学阶段建议先用第一种：应用层显式写入 `DateTimeOffset.UtcNow`。这样更容易在测试里断言，也和其他实体配置保持一致。

如果你修改了配置类，需要删除刚才生成的 migration 后重新生成：

```bash
dotnet ef migrations remove \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj

dotnet ef migrations add InitialCreate \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj \
  --output-dir Persistence/Migrations
```

如果 `migrations remove` 输出里先出现连接数据库错误，但最后仍然看到：

```text
Removing migration '20260818032055_InitialCreate'.
Removing model snapshot.
Done.
```

说明 migration 文件和 snapshot 已经从代码里移除了。前面的连接错误通常是 EF Core 尝试连接 `appsettings.Development.json` 里的本地 PostgreSQL，用来判断这个 migration 是否已经应用到数据库；本地数据库没启动时会记录错误日志。

注意：删除 migration 文件不等于回滚数据库。如果这个 migration 已经应用到某个真实本地库，要先启动数据库并执行：

```bash
dotnet ef database update 0 \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```

再执行 `migrations remove`。如果只是 Day 04 练习环境，也可以直接删除这个临时开发库，后面重新生成并应用 migration。

## 7. 把 migration 应用到本地数据库

### Step 7.1 确认本地 PostgreSQL 连接串

打开：

```text
src/Api/appsettings.Development.json
```

确认有：

```json
{
  "Postgres": {
    "ConnectionString": "Host=localhost;Port=5432;Database=dotnet_meta_server;Username=postgres;Password=postgres"
  }
}
```

如果你的本机 PostgreSQL 用户名、密码或端口不同，先改成本机可用的连接串。

### Step 7.2 应用 migration

执行：

```bash
dotnet ef database update \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```

验收：

- 命令最后能看到 `Done.`
- 第一次建库时，前面可能出现一行 `An error occurred using the connection to database 'dotnet_meta_server'`。这通常只是数据库还不存在，EF 会先连不上、再创建数据库，然后继续执行。只要最后是 `Done.` 就算成功。
- 如果命令以 `PendingModelChangesWarning` 失败，回到 Step 6.5。

### Step 7.3 查看 migration 列表

执行：

```bash
dotnet ef migrations list \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```

你应该看到类似：

```text
20260818******_InitialCreate
```

如果已经应用到当前数据库，通常不会显示 `Pending`。

## 8. 创建种子数据目录

### Step 8.1 创建目录

执行：

```bash
mkdir -p src/Infrastructure/Persistence/SeedData
```

创建文件：

```text
src/Infrastructure/Persistence/SeedData/DevelopmentSeedData.cs
```

### Step 8.2 写入种子数据类

填入：

```csharp
using System.Text.Json;
using Domain.Entities;
using Domain.Entities.Pipelines.Templates;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.SeedData;

public static class DevelopmentSeedData
{
    public static async Task SeedAsync(MetaServerDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var owner = new User
        {
            UserId = "u001",
            DingTalkUserId = "dt-u001",
            ManagerUserId = "u001",
            ManagerDingTalkUserId = "dt-u001",
            Email = "owner@example.com",
            Name = "owner",
            RealName = "Owner User",
            Mobile = "13800000001",
            Role = 1,
            Status = 1,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var developer = new User
        {
            UserId = "u002",
            DingTalkUserId = "dt-u002",
            ManagerUserId = "u001",
            ManagerDingTalkUserId = "dt-u001",
            Email = "developer@example.com",
            Name = "developer",
            RealName = "Developer User",
            Mobile = "13800000002",
            Role = 2,
            Status = 1,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var application = new Application
        {
            Name = "Meta Web",
            AppKey = "meta-web",
            ProjectType = "fe",
            DeployKey = "meta-web-deploy",
            GitId = 1001,
            RegistryKey = "fe",
            GitName = "meta-web",
            GitRepo = "git@example.com:devops/meta-web.git",
            MainBranch = "main",
            PreBranch = "pre",
            StageBranch = "stage",
            DevBranch = "dev",
            GitNamespaceId = 10,
            TriggerToken = "trigger-token-for-test",
            OwnerUserId = owner.UserId,
            OwnerName = owner.Name,
            Status = 1,
            Remark = "Seed application for integration tests.",
            Ranchers = JsonDocument.Parse("""[{"env":"stage","cluster":"seed-cluster"}]"""),
            CreatedByUserId = owner.UserId,
            CreatedByUserName = owner.Name,
            UpdatedByUserId = owner.UserId,
            UpdatedByUserName = owner.Name,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var subApplication = new SubApplication
        {
            ParentApplication = application,
            Name = "Meta Web SaaS",
            AppKey = "meta-web-saas",
            Platform = "web",
            DeployKey = "meta-web-saas-deploy",
            GitId = 1002,
            RegistryKey = "fe",
            GitName = "meta-web-saas",
            GitRepo = "git@example.com:devops/meta-web-saas.git",
            MainBranch = "main",
            PreBranch = "pre",
            StageBranch = "stage",
            DevBranch = "dev",
            ProdSiteAddress = "https://meta.example.com",
            PreSiteAddress = "https://pre-meta.example.com",
            StageSiteAddress = "https://stage-meta.example.com",
            DevSiteAddress = "https://dev-meta.example.com",
            GitNamespaceId = 10,
            TriggerToken = "sub-trigger-token-for-test",
            Remark = "Seed sub application for integration tests.",
            PublicPath = "/",
            UploadToOss = false,
            AppType = "saas",
            Variables = JsonDocument.Parse("""{"NODE_ENV":"test","VITE_API_BASE":"/api"}"""),
            CreatedByUserId = owner.UserId,
            CreatedByUserName = owner.Name,
            UpdatedByUserId = owner.UserId,
            UpdatedByUserName = owner.Name,
            CreatedAt = now,
            UpdatedAt = now,
        };
        application.SubApplications.Add(subApplication);

        var requirement = new Requirement
        {
            Name = "Seed requirement",
            Status = 1,
            DocumentUrl = "https://example.com/docs/seed-requirement",
            Priority = 1,
            Remark = "Seed requirement for list/detail tests.",
            OnlineAt = now.AddDays(7),
            SubmittedTestAt = now.AddDays(3),
            CreatedByUserId = owner.UserId,
            CreatedByUserName = owner.Name,
            UpdatedByUserId = owner.UserId,
            UpdatedByUserName = owner.Name,
            CreatedAt = now,
            UpdatedAt = now,
        };
        requirement.Developers.Add(developer);
        requirement.Followers.Add(owner);

        var iteration = new Iteration
        {
            Application = application,
            SubApplication = subApplication,
            Name = "seed-iteration-001",
            ApplicationName = application.Name,
            SubApplicationName = subApplication.Name,
            Branch = "feature/seed-iteration-001",
            OriginalCommit = "1111111111111111111111111111111111111111",
            Status = 1,
            Remark = "Seed iteration for pipeline tests.",
            CreatedByUserId = owner.UserId,
            CreatedByUserName = owner.Name,
            UpdatedByUserId = owner.UserId,
            UpdatedByUserName = owner.Name,
            CreatedAt = now,
            UpdatedAt = now,
        };
        iteration.Requirements.Add(requirement);

        var template = new PipelineTemplate
        {
            Name = "Frontend Default Pipeline",
            TemplateKey = "frontend-default",
            Status = 1,
            CreatedByUserId = owner.UserId,
            CreatedByUserName = owner.Name,
            UpdatedByUserId = owner.UserId,
            UpdatedByUserName = owner.Name,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var buildStage = new PipelineTemplateStage
        {
            PipelineTemplate = template,
            Name = "Build",
            Seq = 1,
        };

        buildStage.Jobs.Add(new PipelineTemplateJob
        {
            PipelineTemplateStage = buildStage,
            Name = "Install dependencies",
            JobKey = "install",
            StageSeq = 1,
            Extra = JsonDocument.Parse("""{"command":"pnpm install"}"""),
        });

        buildStage.Jobs.Add(new PipelineTemplateJob
        {
            PipelineTemplateStage = buildStage,
            Name = "Build artifact",
            JobKey = "build",
            StageSeq = 1,
            Extra = JsonDocument.Parse("""{"command":"pnpm build"}"""),
        });

        var deployStage = new PipelineTemplateStage
        {
            PipelineTemplate = template,
            Name = "Deploy",
            Seq = 2,
        };

        deployStage.Jobs.Add(new PipelineTemplateJob
        {
            PipelineTemplateStage = deployStage,
            Name = "Deploy to stage",
            JobKey = "deploy-stage",
            StageSeq = 2,
            Extra = JsonDocument.Parse("""{"env":"stage"}"""),
        });

        template.Stages.Add(buildStage);
        template.Stages.Add(deployStage);
        application.PipelineTemplates.Add(template);
        subApplication.PipelineTemplates.Add(template);

        dbContext.Users.AddRange(owner, developer);
        dbContext.Applications.Add(application);
        dbContext.Requirements.Add(requirement);
        dbContext.Iterations.Add(iteration);
        dbContext.PipelineTemplates.Add(template);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

### Step 8.3 先理解为什么不用 `HasData`

EF Core 也支持在模型配置里写：

```csharp
builder.HasData(...);
```

今天不采用它，原因是这批数据主要服务集成测试和开发环境，不是数据库结构的一部分。把种子数据写成普通 C# 方法有几个好处：

- 可以使用 navigation 关系，代码更接近业务对象。
- 可以用 `DateTimeOffset.UtcNow` 生成时间，不需要把固定时间写进 migration。
- 测试可以选择是否执行种子数据。
- 后续能拆分成 `TestSeedData`、`DevelopmentSeedData`、`DemoSeedData`。

### Step 8.4 检查命名空间

执行：

```bash
dotnet build
```

验收：

- 如果报 `The type or namespace name 'SeedData' could not be found`，检查文件路径和命名空间是不是：

```csharp
namespace Infrastructure.Persistence.SeedData;
```

## 9. 安装集成测试依赖

### Step 9.1 给 IntegrationTests 添加包

执行：

```bash
dotnet add tests/IntegrationTests/IntegrationTests.csproj package Testcontainers.PostgreSql
dotnet add tests/IntegrationTests/IntegrationTests.csproj package Testcontainers.Redis
dotnet add tests/IntegrationTests/IntegrationTests.csproj package StackExchange.Redis
```

这些包先这样理解：

- `Testcontainers.PostgreSql`：启动临时 PostgreSQL 容器。
- `Testcontainers.Redis`：启动临时 Redis 容器。
- `StackExchange.Redis`：在测试里连接 Redis，执行 set/get。

### Step 9.2 显式引用 Infrastructure

执行：

```bash
dotnet add tests/IntegrationTests/IntegrationTests.csproj reference src/Infrastructure/Infrastructure.csproj
```

`IntegrationTests` 虽然已经引用了 `Api`，但今天测试会直接拿 `MetaServerDbContext` 和 `DevelopmentSeedData` 做断言。显式引用 `Infrastructure` 可以让测试意图更清楚。

### Step 9.3 restore

执行：

```bash
dotnet restore
```

验收：

- restore 成功，没有红色错误。

## 10. 创建测试环境 Fixture

### Step 10.1 创建测试支持目录

执行：

```bash
mkdir -p tests/IntegrationTests/Support
```

创建文件：

```text
tests/IntegrationTests/Support/TestEnvironmentFixture.cs
```

### Step 10.2 写入 PostgreSQL 和 Redis 容器生命周期

填入：

```csharp
using Infrastructure.Persistence;
using Infrastructure.Persistence.SeedData;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace IntegrationTests.Support;

public sealed class TestEnvironmentFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("dotnet_meta_server_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public string PostgresConnectionString => _postgres.GetConnectionString();

    public string RedisConnectionString => _redis.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _redis.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    var overrides = new Dictionary<string, string?>
                    {
                        ["Postgres:ConnectionString"] = PostgresConnectionString,
                        ["Redis:ConnectionString"] = RedisConnectionString,
                    };

                    configuration.AddInMemoryCollection(overrides);
                });
            });

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MetaServerDbContext>();
        await dbContext.Database.MigrateAsync();
        await DevelopmentSeedData.SeedAsync(dbContext);
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _redis.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
```

### Step 10.3 检查 using

最终 using 应该是：

```csharp
using Infrastructure.Persistence;
using Infrastructure.Persistence.SeedData;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
```

如果 `WebApplicationFactory<Program>` 报找不到类型，通常是漏了 `Microsoft.AspNetCore.Mvc.Testing`。

### Step 10.4 先理解测试生命周期

这段 fixture 做了 5 件事：

```text
InitializeAsync
  1. 启动 PostgreSQL 容器
  2. 启动 Redis 容器
  3. 用容器连接串覆盖 appsettings.Development.json
  4. 执行 EF Core migration
  5. 写入种子数据

DisposeAsync
  1. 释放测试 API
  2. 删除 Redis 容器
  3. 删除 PostgreSQL 容器
```

这就是集成测试的关键：每次测试都从可预期的环境开始，不依赖你电脑上昨天留下来的数据库状态。

## 11. 创建数据库集成测试

### Step 11.1 创建 `DatabaseIntegrationTests.cs`

创建文件：

```text
tests/IntegrationTests/DatabaseIntegrationTests.cs
```

填入：

```csharp
using Infrastructure.Persistence;
using IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public sealed class DatabaseIntegrationTests : IClassFixture<TestEnvironmentFixture>
{
    private readonly TestEnvironmentFixture _fixture;

    public DatabaseIntegrationTests(TestEnvironmentFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Database_can_connect_after_migrations_are_applied()
    {
        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MetaServerDbContext>();

        var canConnect = await dbContext.Database.CanConnectAsync();

        Assert.True(canConnect);
    }

    [Fact]
    public async Task Seed_data_contains_core_objects_for_later_modules()
    {
        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MetaServerDbContext>();

        var userCount = await dbContext.Users.CountAsync();
        var application = await dbContext.Applications
            .Include(item => item.SubApplications)
            .SingleAsync(item => item.AppKey == "meta-web");
        var requirement = await dbContext.Requirements
            .Include(item => item.Developers)
            .Include(item => item.Followers)
            .SingleAsync(item => item.Name == "Seed requirement");
        var iteration = await dbContext.Iterations
            .Include(item => item.Requirements)
            .SingleAsync(item => item.Branch == "feature/seed-iteration-001");
        var template = await dbContext.PipelineTemplates
            .Include(item => item.Stages)
            .ThenInclude(item => item.Jobs)
            .SingleAsync(item => item.TemplateKey == "frontend-default");

        Assert.Equal(2, userCount);
        Assert.Single(application.SubApplications);
        Assert.Single(requirement.Developers);
        Assert.Single(requirement.Followers);
        Assert.Single(iteration.Requirements);
        Assert.Equal(2, template.Stages.Count);
        Assert.Equal(3, template.Stages.SelectMany(item => item.Jobs).Count());
    }
}
```

### Step 11.2 运行数据库测试

执行：

```bash
dotnet test tests/IntegrationTests/IntegrationTests.csproj \
  --filter FullyQualifiedName~DatabaseIntegrationTests
```

第一次运行可能会比较慢，因为 Docker 需要拉取镜像：

```text
postgres:16-alpine
redis:7-alpine
```

验收：

- 两个测试都通过。
- 本机没有启动 PostgreSQL 服务也没关系，因为测试使用的是容器数据库。

## 12. 创建 Redis 集成测试

### Step 12.1 创建 `RedisIntegrationTests.cs`

创建文件：

```text
tests/IntegrationTests/RedisIntegrationTests.cs
```

填入：

```csharp
using IntegrationTests.Support;
using StackExchange.Redis;

namespace IntegrationTests;

public sealed class RedisIntegrationTests : IClassFixture<TestEnvironmentFixture>
{
    private readonly TestEnvironmentFixture _fixture;

    public RedisIntegrationTests(TestEnvironmentFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Redis_can_set_and_get_value()
    {
        await using var connection = await ConnectionMultiplexer.ConnectAsync(_fixture.RedisConnectionString);
        var database = connection.GetDatabase();

        await database.StringSetAsync("day04:redis:ping", "pong");
        var value = await database.StringGetAsync("day04:redis:ping");

        Assert.Equal("pong", value);
    }
}
```

### Step 12.2 运行 Redis 测试

执行：

```bash
dotnet test tests/IntegrationTests/IntegrationTests.csproj \
  --filter FullyQualifiedName~RedisIntegrationTests
```

验收：

- 测试通过。
- 如果失败并提示 Docker 连接不上，先回到 Step 2 检查 Docker。

## 13. 改造健康检查集成测试

Day 02 的健康检查测试使用 `WebApplicationFactory<Program>` 默认 fixture。今天要让它也跑在 Testcontainers 环境里，这样整个 `IntegrationTests` 项目都不依赖本机 PostgreSQL/Redis。

### Step 13.1 修改 `ApiSmokeTests.cs`

打开：

```text
tests/IntegrationTests/ApiSmokeTests.cs
```

改成：

```csharp
using System.Net;
using IntegrationTests.Support;

namespace IntegrationTests;

public class ApiSmokeTests : IClassFixture<TestEnvironmentFixture>
{
    private readonly TestEnvironmentFixture _fixture;

    public ApiSmokeTests(TestEnvironmentFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

### Step 13.2 运行健康检查测试

执行：

```bash
dotnet test tests/IntegrationTests/IntegrationTests.csproj \
  --filter FullyQualifiedName~ApiSmokeTests
```

验收：

- `/health` 返回 200。
- 测试不读取本机 `localhost:5432` 或 `localhost:6379`。

## 14. 处理诊断接口测试

`DiagnosticsApiTests` 目前测试统一响应、业务异常、未知异常、401、模型校验和 requestId。它不主动访问数据库，但 `Program.cs` 会注册 `MetaServerDbContext`，所以最好也让它使用同一个 Testcontainers fixture。

### Step 14.1 修改 `DiagnosticsApiTests.cs`

打开：

```text
tests/IntegrationTests/DiagnosticsApiTests.cs
```

把：

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
```

改成：

```csharp
using IntegrationTests.Support;
```

再把类声明和构造函数从：

```csharp
public class DiagnosticsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DiagnosticsApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
}
```

改成：

```csharp
public class DiagnosticsApiTests : IClassFixture<TestEnvironmentFixture>
{
    private readonly TestEnvironmentFixture _fixture;

    public DiagnosticsApiTests(TestEnvironmentFixture fixture)
    {
        _fixture = fixture;
    }
}
```

然后把文件中的：

```csharp
_factory.CreateClient()
```

全部替换成：

```csharp
_fixture.Factory.CreateClient()
```

### Step 14.2 运行诊断接口测试

执行：

```bash
dotnet test tests/IntegrationTests/IntegrationTests.csproj \
  --filter FullyQualifiedName~DiagnosticsApiTests
```

验收：

- Day 02 的统一响应测试仍然通过。
- 说明切换测试环境没有破坏 API 管线。

## 15. 让同一个测试集合共享容器

如果每个测试类都各自实现 `IClassFixture<TestEnvironmentFixture>`，xUnit 会按测试类创建 fixture。数据库测试、Redis 测试、健康检查测试可能各自启动一套容器，能跑但比较慢。

今天可以把它升级成 collection fixture，让整个集成测试项目共享一套 PostgreSQL/Redis 容器。

### Step 15.1 创建 `IntegrationTestCollection.cs`

创建文件：

```text
tests/IntegrationTests/Support/IntegrationTestCollection.cs
```

填入：

```csharp
namespace IntegrationTests.Support;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<TestEnvironmentFixture>
{
    public const string Name = "Integration test collection";
}
```

### Step 15.2 修改测试类标记

把这些测试类：

```text
tests/IntegrationTests/ApiSmokeTests.cs
tests/IntegrationTests/DatabaseIntegrationTests.cs
tests/IntegrationTests/DiagnosticsApiTests.cs
tests/IntegrationTests/RedisIntegrationTests.cs
```

从：

```csharp
public sealed class DatabaseIntegrationTests : IClassFixture<TestEnvironmentFixture>
```

改成：

```csharp
[Collection(IntegrationTestCollection.Name)]
public sealed class DatabaseIntegrationTests
```

如果类不是 `sealed`，也可以保持原样，只要删除 `IClassFixture<TestEnvironmentFixture>` 即可。例如：

```csharp
[Collection(IntegrationTestCollection.Name)]
public class ApiSmokeTests
```

每个文件顶部都要有：

```csharp
using IntegrationTests.Support;
```

构造函数仍然接收 fixture：

```csharp
private readonly TestEnvironmentFixture _fixture;

public DatabaseIntegrationTests(TestEnvironmentFixture fixture)
{
    _fixture = fixture;
}
```

你可以先这样理解：

- `IClassFixture<T>`：一个测试类一套 fixture。
- `ICollectionFixture<T>`：一个测试集合一套 fixture。
- 容器启动很慢，所以集成测试更适合 collection fixture。

## 16. 跑完整测试

### Step 16.1 先 build

执行：

```bash
dotnet build
```

验收：

- `Build succeeded.`

### Step 16.2 跑全部测试

执行：

```bash
dotnet test
```

验收：

- UnitTests 通过。
- IntegrationTests 通过。
- 不需要你手动启动本机 PostgreSQL。
- 不需要你手动启动本机 Redis。

### Step 16.3 如果测试卡在拉镜像

第一次运行时，Testcontainers 会拉：

```text
postgres:16-alpine
redis:7-alpine
```

可以单独执行：

```bash
docker pull postgres:16-alpine
docker pull redis:7-alpine
```

再重新运行：

```bash
dotnet test tests/IntegrationTests/IntegrationTests.csproj
```

## 17. 常见错误

### 17.1 `dotnet ef` 不是命令

现象：

```text
Could not execute because the specified command or file was not found.
```

处理：

```bash
dotnet tool restore
dotnet tool list
```

如果清单里没有 `dotnet-ef`：

```bash
dotnet tool install dotnet-ef --version '10.*'
```

### 17.2 startup project 没有 Design 包

现象：

```text
Your startup project 'Api' doesn't reference Microsoft.EntityFrameworkCore.Design. This package is required for the Entity Framework Core Tools to work. Ensure your startup project is correct, install the package, and try again.
```

原因：

`dotnet ef` 要求 **startup project** 直接引用 `Microsoft.EntityFrameworkCore.Design`。Day 03 把这个包加在了 `Infrastructure` 上，并且带了 `PrivateAssets=all`，所以不会传到 `Api`。`--project` 指向 `Infrastructure` 并不能满足这项检查。

处理：

```bash
dotnet add src/Api/Api.csproj package Microsoft.EntityFrameworkCore.Design --version 10.0.11
```

然后重新执行原来的 `dotnet ef` 命令。

### 17.3 startup project 找不到配置

现象：

```text
Value cannot be null. (Parameter 'connectionString')
```

处理：

确认 EF 命令带上了：

```bash
--startup-project src/Api/Api.csproj
```

因为连接串在 `Api` 的配置文件里，不在 `Infrastructure` 的配置文件里。

### 17.4 migration 生成了意料外的表名

现象：

```text
CREATE TABLE "Application"
```

或：

```text
CREATE TABLE "PipelineTemplate"
```

处理：

回到对应的 `IEntityTypeConfiguration<T>`，确认写了：

```csharp
builder.ToTable("applications");
```

并且 `MetaServerDbContext.OnModelCreating` 中有：

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(MetaServerDbContext).Assembly);
```

然后删除 migration，重新生成。

### 17.5 Testcontainers 连接不上 Docker

现象：

```text
Docker is either not running or misconfigured
```

处理：

```bash
docker info
```

如果 Docker 没启动，打开 Docker Desktop。启动后重新跑：

```bash
dotnet test tests/IntegrationTests/IntegrationTests.csproj
```

### 17.6 PostgreSQL migration 失败

现象：

```text
relation ... already exists
```

处理：

本地库可能已经有旧表。学习阶段可以换一个空数据库名，例如把本地连接串里的：

```text
Database=dotnet_meta_server
```

临时改成：

```text
Database=dotnet_meta_server_day04
```

然后重新执行：

```bash
dotnet ef database update \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj
```

集成测试里一般不会遇到这个问题，因为 Testcontainers 每次都是新数据库。

### 17.7 模型每次构建都变化

现象：

```text
PendingModelChangesWarning: The model for context 'MetaServerDbContext' changes each time it is built.
This is usually caused by dynamic values used in a 'HasData' call (e.g. `new DateTime()`, `Guid.NewGuid()`).
```

日志里可能同时出现：

```text
An error occurred using the connection to database 'dotnet_meta_server' on server 'tcp://localhost:5432'.
```

先处理模型问题。连接失败常常只是附带日志；真正让命令退出的，是模型每次构建结果都不一样。

原因：

配置类里写了动态默认值，例如：

```csharp
.HasDefaultValue(DateTimeOffset.UtcNow)
```

处理：

1. 删掉时间字段上的 `HasDefaultValue(DateTimeOffset.UtcNow)`，只保留列名和 `timestamp with time zone`。
2. 删除尚未应用到数据库的 `InitialCreate`，再重新生成：

```bash
dotnet ef migrations remove \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj

dotnet ef migrations add InitialCreate \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Api/Api.csproj \
  --output-dir Persistence/Migrations
```

3. 再执行 `dotnet ef database update`。第一次建库时，仍可能看到 `An error occurred using the connection...`，但只要最后打印 `Done.` 就说明已经应用成功。

如果命令连 `Done.` 都没有，并且明确连不上端口，再检查本机 PostgreSQL 是否已启动，以及 `src/Api/appsettings.Development.json` 里的连接串是否正确。

## 18. 今天的提交建议

完成后先看改动：

```bash
git status --short
```

建议至少包含这些文件：

```text
.config/dotnet-tools.json
src/Api/Api.csproj
src/Infrastructure/Persistence/Migrations/*
src/Infrastructure/Persistence/SeedData/DevelopmentSeedData.cs
tests/IntegrationTests/IntegrationTests.csproj
tests/IntegrationTests/Support/TestEnvironmentFixture.cs
tests/IntegrationTests/Support/IntegrationTestCollection.cs
tests/IntegrationTests/DatabaseIntegrationTests.cs
tests/IntegrationTests/RedisIntegrationTests.cs
tests/IntegrationTests/ApiSmokeTests.cs
tests/IntegrationTests/DiagnosticsApiTests.cs
```

执行：

```bash
git add .config/dotnet-tools.json \
  src/Api/Api.csproj \
  src/Infrastructure/Persistence/Migrations \
  src/Infrastructure/Persistence/SeedData/DevelopmentSeedData.cs \
  tests/IntegrationTests

git commit -m "test: add repeatable database integration environment"
```

如果你把 `artifacts/migrations/initial-create.sql` 也提交了，提交信息可以改成：

```bash
git commit -m "test: add migrations and containerized integration tests"
```

`artifacts/migrations/initial-create.sql` 是否提交由团队习惯决定。migration C# 文件必须提交，临时生成的 SQL 可以只用于 review。

## 19. 今天你应该理解的概念

### 19.1 Migration 不是“自动同步数据库”

Migration 是一份版本化的结构变更记录。模型变了以后，你需要显式生成 migration，再检查它，再应用到数据库。

不要在生产项目里依赖“启动时自动建表”来替代 migration。测试环境可以自动 `MigrateAsync()`，但生产环境通常要通过发布流程、DBA 审核或 migration bundle 来控制。

### 19.2 Testcontainers 不是 mock

Testcontainers 启动的是真 PostgreSQL、真 Redis。它比 in-memory fake 慢，但能发现更多真实问题：

- PostgreSQL 类型映射是否正确。
- migration 是否真的能执行。
- Redis 连接串格式是否正确。
- API 启动时依赖注入是否完整。

### 19.3 种子数据要小而稳定

今天的种子数据故意只放：

- 2 个用户。
- 1 个应用。
- 1 个子应用。
- 1 个需求。
- 1 个迭代。
- 1 个流水线模板。
- 2 个阶段。
- 3 个任务。

这批数据足够支撑后续列表、详情、筛选、关联查询测试。不要一开始塞很多“看起来真实”的数据，测试会变慢，也更难判断失败原因。

## 20. 晚上复盘

可以按这几个问题写学习笔记：

- 今天学会的 C#/.NET 概念：
- `dotnet-ef` 的 `--project` 和 `--startup-project` 分别是什么意思：
- `Microsoft.EntityFrameworkCore.Design` 为什么必须加在 `Api`，而不是只加在 `Infrastructure`：
- EF Core migration 文件里最应该人工检查的 5 个点：
- Testcontainers 的初始化和清理生命周期：
- 种子数据为什么要保持小而稳定：
- 明天风险：

## 参考资料

### 当前项目参考

- `docs/superpowers/plans/dotnet-meta-server-40-days/day-01-step-by-step.md`
- `docs/superpowers/plans/dotnet-meta-server-40-days/day-02-step-by-step.md`
- `docs/superpowers/plans/dotnet-meta-server-40-days/day-03-step-by-step.md`
- `src/Infrastructure/Persistence/MetaServerDbContext.cs`
- `src/Infrastructure/Persistence/Configurations/*`
- `tests/UnitTests/Persistence/MetaServerDbContextMetadataTests.cs`

### 原 NestJS 业务源码参考

- `/Users/fenghe/workspace/devops/meta-server/src/core/entities/user.entity.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/core/entities/application.entity.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/core/entities/sub.application.entity.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/core/entities/requirement.entity.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/core/entities/iteration.entity.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/core/entities/pipeline/template/pipeline.tpl.entity.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/core/entities/pipeline/template/pipeline.tpl.stage.entity.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/core/entities/pipeline/template/pipeline.tpl.job.entity.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/core/entities/pipeline/processor/pipeline.entity.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/core/entities/pipeline/processor/pipeline.job.entity.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/core/entities/deploy.entity.ts`
- `/Users/fenghe/workspace/devops/meta-server/src/core/entities/monitor.entity.ts`

### 官方文档

- Microsoft Learn：EF Core migrations overview  
  `https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/`
- Microsoft Learn：EF Core tools reference for .NET CLI  
  `https://learn.microsoft.com/en-us/ef/core/cli/dotnet`
- Testcontainers for .NET：PostgreSQL module  
  `https://dotnet.testcontainers.org/modules/postgres/`
- Testcontainers for .NET：Redis module  
  `https://dotnet.testcontainers.org/modules/redis/`
