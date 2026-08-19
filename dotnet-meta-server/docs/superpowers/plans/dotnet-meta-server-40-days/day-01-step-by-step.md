# Day 01 Step-by-Step - 解决方案骨架与 C# 快速上手

这份文档是 `day-01.md` 的跟做版。你可以从上到下照着执行，每完成一小步就打勾。今天的目标不是理解所有 .NET 细节，而是把项目骨架跑起来，并知道每个目录大概负责什么。

## 0. 今天最终要得到什么

完成后，你的目录大概长这样：

```text
dotnet-meta-server/
├── Directory.Build.props
├── DotnetMetaServer.slnx  # .NET 10 默认生成；.NET 9 或更早可能是 DotnetMetaServer.sln
├── src/
│   ├── Api/
│   ├── Application/
│   ├── Domain/
│   └── Infrastructure/
└── tests/
    ├── UnitTests/
    └── IntegrationTests/
```

你最后需要确认三件事：

- `dotnet build` 成功。
- `dotnet test` 成功。
- 浏览器能打开 Swagger，访问 `/health` 能看到成功响应。

## 1. 准备 .NET SDK

### Step 1.1 检查电脑上有没有 dotnet

打开终端，执行：

```bash
dotnet --version
```

如果看到类似下面的版本号，说明已经安装：

```text
8.0.***
```

如果看到：

```text
command not found: dotnet
```

说明还没有安装 .NET SDK。先安装 .NET SDK 8 或更高版本，然后重新打开终端，再执行一次 `dotnet --version`。

macOS 如果你用 Homebrew，可以尝试：

```bash
brew install --cask dotnet-sdk
```

验收：

- 能执行 `dotnet --version`。
- 输出是一个版本号，不再是 `command not found`。

## 2. 进入项目目录

### Step 2.1 切到 `dotnet-meta-server`

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

如果不是这个路径，先不要继续，重新执行上面的 `cd` 命令。

## 3. 创建解决方案文件

### Step 3.1 创建 `.sln`

执行：

```bash
dotnet new sln -n DotnetMetaServer
```

这会创建解决方案文件。不同 .NET SDK 版本生成的文件名不一样：

```text
.NET 10：DotnetMetaServer.slnx
.NET 9 或更早：DotnetMetaServer.sln
```

你可以检查一下：

```bash
ls
```

验收：

- 当前目录下能看到 `DotnetMetaServer.slnx` 或 `DotnetMetaServer.sln`。

你可以先这样理解：

- `.slnx` / `.sln` 是总清单，负责把多个项目放在一起。
- `.csproj` 是单个项目自己的配置文件。

## 4. 创建源码和测试目录

### Step 4.1 创建目录

执行：

```bash
mkdir -p src tests
```

验收：

```bash
ls
```

你应该能看到：

```text
DotnetMetaServer.slnx
docs
src
tests
```

如果你的 SDK 生成的是旧格式，这里会显示 `DotnetMetaServer.sln`，也没问题。

## 5. 创建 4 个源码项目

### Step 5.1 创建 Api 项目

执行：

```bash
dotnet new webapi -n Api -o src/Api
```

`Api` 是对外入口，后面浏览器、前端、第三方系统都会先访问它。

### Step 5.2 创建 Application 项目

执行：

```bash
dotnet new classlib -n Application -o src/Application
```

`Application` 放应用用例和业务流程，比如“创建用户”“提交订单”“查询列表”。

这个项目名会成为根命名空间 `Application`。Day 03 会再有一个同名实体类 `Application`。C# 里同一个简单名字不能既是命名空间又是类型，所以从 Day 03 起，Infrastructure 和测试里会用别名 `AppEntity` 引用那个实体。现在先记住这件事，Day 03 会真正写出来。

### Step 5.3 创建 Domain 项目

执行：

```bash
dotnet new classlib -n Domain -o src/Domain
```

`Domain` 放最核心的业务概念，比如实体、值对象、领域规则。它应该尽量少依赖外部技术。

### Step 5.4 创建 Infrastructure 项目

执行：

```bash
dotnet new classlib -n Infrastructure -o src/Infrastructure
```

`Infrastructure` 放外部技术细节，比如数据库、缓存、文件、第三方接口。

### Step 5.5 检查源码项目

执行：

```bash
find src -maxdepth 2 -name "*.csproj"
```

你应该看到：

```text
src/Api/Api.csproj
src/Application/Application.csproj
src/Domain/Domain.csproj
src/Infrastructure/Infrastructure.csproj
```

## 6. 创建 2 个测试项目

### Step 6.1 创建 UnitTests

执行：

```bash
dotnet new xunit -n UnitTests -o tests/UnitTests
```

`UnitTests` 放单元测试。单元测试通常测试一个类、一个函数、一个业务规则。

### Step 6.2 创建 IntegrationTests

执行：

```bash
dotnet new xunit -n IntegrationTests -o tests/IntegrationTests
```

`IntegrationTests` 放集成测试。今天先写一个最小测试：启动 API 测试服务器，然后请求 `/health`。

### Step 6.3 检查测试项目

执行：

```bash
find tests -maxdepth 2 -name "*.csproj"
```

你应该看到：

```text
tests/IntegrationTests/IntegrationTests.csproj
tests/UnitTests/UnitTests.csproj
```

## 7. 把项目加入解决方案

### Step 7.1 一次性加入所有项目

执行：

```bash
dotnet sln add \
  src/Api/Api.csproj \
  src/Application/Application.csproj \
  src/Domain/Domain.csproj \
  src/Infrastructure/Infrastructure.csproj \
  tests/UnitTests/UnitTests.csproj \
tests/IntegrationTests/IntegrationTests.csproj
```

这里故意不写 `DotnetMetaServer.sln` 或 `DotnetMetaServer.slnx`。只要当前目录下只有一个解决方案文件，`dotnet sln` 会自动找到它。

如果你的目录里同时有多个解决方案文件，再显式指定：

```bash
dotnet sln DotnetMetaServer.slnx add \
  src/Api/Api.csproj \
  src/Application/Application.csproj \
  src/Domain/Domain.csproj \
  src/Infrastructure/Infrastructure.csproj \
  tests/UnitTests/UnitTests.csproj \
  tests/IntegrationTests/IntegrationTests.csproj
```

如果你用的是 `.sln`，就把上面的 `DotnetMetaServer.slnx` 换成 `DotnetMetaServer.sln`。

### Step 7.2 检查解决方案里有哪些项目

执行：

```bash
dotnet sln list
```

你应该能看到 6 个 `.csproj`。

如果少了某一个项目，重新执行一次 `dotnet sln ... add`，只添加缺少的那个项目即可。

## 8. 建立项目引用关系

### Step 8.1 让 Api 引用 Application

执行：

```bash
dotnet add src/Api/Api.csproj reference src/Application/Application.csproj
```

意思是：API 层可以调用应用层。

### Step 8.2 让 Application 引用 Domain

执行：

```bash
dotnet add src/Application/Application.csproj reference src/Domain/Domain.csproj
```

意思是：应用层可以使用领域层的业务对象和规则。

### Step 8.3 让 Infrastructure 引用 Application 和 Domain

执行：

```bash
dotnet add src/Infrastructure/Infrastructure.csproj reference \
  src/Application/Application.csproj \
  src/Domain/Domain.csproj
```

意思是：基础设施层可以实现应用层需要的接口，也可以保存领域对象。

### Step 8.4 让 UnitTests 引用 Application 和 Domain

执行：

```bash
dotnet add tests/UnitTests/UnitTests.csproj reference \
  src/Application/Application.csproj \
  src/Domain/Domain.csproj
```

### Step 8.5 让 IntegrationTests 引用 Api

执行：

```bash
dotnet add tests/IntegrationTests/IntegrationTests.csproj reference src/Api/Api.csproj
```

今天先只让集成测试引用 `Api`，因为 smoke test 要启动 API 测试服务器。

## 9. 创建统一构建配置

### Step 9.1 创建 `Directory.Build.props`

在 `dotnet-meta-server` 目录下创建文件：

```text
Directory.Build.props
```

填入下面内容：

```xml
<Project>
  <PropertyGroup>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
</Project>
```

你可以先这样理解：

- `Nullable enable`：让 C# 帮你提醒“这个变量可能是 null”。
- `ImplicitUsings enable`：让常用 `using` 自动生效，少写一些样板代码。
- `LangVersion latest`：使用当前 SDK 支持的最新 C# 语法。

### Step 9.2 检查文件是否创建成功

执行：

```bash
cat Directory.Build.props
```

能看到刚才那段 XML 就可以继续。

## 10. 配置 Api 的 Swagger 和 `/health`

### Step 10.1 打开 `src/Api/Program.cs`

找到这个文件：

```text
src/Api/Program.cs
```

把内容替换为：

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok"
}));

app.Run();

public partial class Program
{
}
```

这里你先记住三件事：

- `builder.Services...` 是注册服务，先告诉程序“我有哪些能力”。
- `app.Use...` 是请求管线，请求进来后会按顺序经过这些处理。
- `app.MapGet("/health", ...)` 是定义一个 HTTP GET 接口。

### Step 10.2 如果 `AddSwaggerGen` 报错

如果后面 build 时提示找不到 `AddSwaggerGen`，执行：

```bash
dotnet add src/Api/Api.csproj package Swashbuckle.AspNetCore
```

然后再继续。

## 11. 写最小集成测试

### Step 11.1 给 IntegrationTests 添加测试服务器依赖

先查看 API 项目的目标框架：

```bash
grep TargetFramework src/Api/Api.csproj
```

你会看到类似：

```xml
<TargetFramework>net8.0</TargetFramework>
```

然后按你的版本选择一个命令。

如果是 `net8.0`，执行：

```bash
dotnet add tests/IntegrationTests/IntegrationTests.csproj package Microsoft.AspNetCore.Mvc.Testing --version 8.*
```

如果是 `net9.0`，执行：

```bash
dotnet add tests/IntegrationTests/IntegrationTests.csproj package Microsoft.AspNetCore.Mvc.Testing --version 9.*
```

如果是 `net10.0`，执行：

```bash
dotnet add tests/IntegrationTests/IntegrationTests.csproj package Microsoft.AspNetCore.Mvc.Testing --version 10.*
```

如果你不确定，就先执行不带版本的命令：

```bash
dotnet add tests/IntegrationTests/IntegrationTests.csproj package Microsoft.AspNetCore.Mvc.Testing
```

这个包提供 `WebApplicationFactory`，它可以在测试里启动一个内存中的 API 服务。如果后面 restore 或 build 提示版本不兼容，再回到这里选择和 `TargetFramework` 主版本一致的命令。

### Step 11.2 删除模板自带测试文件

查看模板文件名：

```bash
ls tests/IntegrationTests
```

如果看到 `UnitTest1.cs`，可以删除它：

```bash
rm tests/IntegrationTests/UnitTest1.cs
```

### Step 11.3 创建 smoke test 文件

创建文件：

```text
tests/IntegrationTests/ApiSmokeTests.cs
```

填入：

```csharp
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests;

public class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

你可以先这样理解：

- `async Task`：这个测试里有异步操作。
- `await client.GetAsync("/health")`：发送 HTTP GET 请求。
- `Assert.Equal(...)`：判断实际结果是不是符合预期。

## 12. 整理 UnitTests

### Step 12.1 删除 UnitTests 模板文件

如果存在：

```text
tests/UnitTests/UnitTest1.cs
```

可以删除：

```bash
rm tests/UnitTests/UnitTest1.cs
```

### Step 12.2 创建一个最小单元测试

创建文件：

```text
tests/UnitTests/ProjectSmokeTests.cs
```

填入：

```csharp
namespace UnitTests;

public class ProjectSmokeTests
{
    [Fact]
    public void UnitTestProject_IsConfigured()
    {
        Assert.True(true);
    }
}
```

这个测试没有业务意义，只是确认测试项目本身能跑。后面学到业务代码后，再把它替换成真正的单元测试。

## 13. 第一次构建

### Step 13.1 执行 build

在 `dotnet-meta-server` 目录执行：

```bash
dotnet build
```

成功时，你会看到类似：

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

如果失败，先看最后几行错误：

- 如果是 `dotnet: command not found`，回到第 1 步安装 SDK。
- 如果是 `AddSwaggerGen` 找不到，回到 Step 10.2 添加 Swagger 包。
- 如果是 `Program` 访问不到，确认 `src/Api/Program.cs` 最底部有 `public partial class Program`。

## 14. 运行测试

### Step 14.1 执行 test

执行：

```bash
dotnet test
```

成功时，你应该看到类似：

```text
Passed!
```

今天至少应该有两个测试通过：

- `ProjectSmokeTests.UnitTestProject_IsConfigured`
- `ApiSmokeTests.Health_ReturnsOk`

如果测试失败，先执行更详细的命令：

```bash
dotnet test --logger "console;verbosity=detailed"
```

然后看失败测试名和错误信息。

## 15. 启动 API

### Step 15.1 运行 Api 项目

执行：

```bash
dotnet run --project src/Api/Api.csproj
```

启动成功后，终端会显示类似：

```text
Now listening on: http://localhost:****
Now listening on: https://localhost:****
```

注意这里的端口号可能每个人不一样。下面用 `http://localhost:****` 表示你终端里实际看到的地址。

这个命令会一直占用当前终端，不要关闭它。你要测试接口时，另外打开一个新终端。

## 16. 验证 `/health`

### Step 16.1 用浏览器验证

打开浏览器，访问：

```text
http://localhost:你的端口号/health
```

你应该看到类似：

```json
{"status":"ok"}
```

### Step 16.2 或者用 curl 验证

新开一个终端，执行：

```bash
curl http://localhost:你的端口号/health
```

你应该看到：

```json
{"status":"ok"}
```

## 17. 验证 Swagger

### Step 17.1 打开 Swagger 页面

浏览器访问：

```text
http://localhost:你的端口号/swagger
```

你应该看到 Swagger UI 页面。

如果打不开，检查三件事：

- `dotnet run` 那个终端还在运行。
- 端口号和终端显示的一样。
- `Program.cs` 里有 `app.UseSwagger();` 和 `app.UseSwaggerUI();`。

## 18. 记录今天学到的 C# 语法

在这个文件里记录：

```text
docs/learning-notes/day-01-csharp-basics.md
```

如果文件不存在，就创建它。可以先写下面这版：

```markdown
# Day 01 - C# 基础记录

## namespace

`namespace IntegrationTests;` 表示这个文件里的类属于 `IntegrationTests` 命名空间，用来组织代码，避免类名冲突。

## using

`using System.Net;` 表示引入别的命名空间。引入后可以直接写 `HttpStatusCode`。

## public

`public` 表示公开，别的项目或类可以访问。

## class

`class ApiSmokeTests` 定义一个类。类里面可以放字段、构造函数、方法。

## record

今天还没有必须使用 record。可以先理解成更适合表达数据的 class，后面写 DTO 或值对象时再练。

## interface

今天还没有必须使用 interface。可以先理解成“约定”：它只规定有哪些方法，不关心具体怎么实现。

## async / await

`async Task` 表示这是异步方法。

`await client.GetAsync("/health")` 表示等待 HTTP 请求完成，再继续执行下面的断言。
```

## 19. 今天的最终验收

按顺序执行下面三组命令。

### Step 19.1 build

```bash
dotnet build
```

通过标准：

```text
Build succeeded.
```

### Step 19.2 test

```bash
dotnet test
```

通过标准：

```text
Passed!
```

### Step 19.3 run

```bash
dotnet run --project src/Api/Api.csproj
```

通过标准：

- `/health` 返回 `{"status":"ok"}`。
- `/swagger` 能打开页面。

## 20. 你需要能说出来的项目分工

完成后，试着用自己的话说下面这段：

```text
Api 是 HTTP 入口，负责接收请求和返回响应。
Application 是应用层，后面会放业务流程。
Domain 是领域层，后面会放核心业务概念和规则。
Infrastructure 是基础设施层，后面会放数据库、缓存、第三方接口等技术细节。
UnitTests 是单元测试。
IntegrationTests 是集成测试，会把 API 服务启动起来做更接近真实请求的测试。
```

如果你能说出这段，Day 1 的理解目标就达到了。

## 21. 常见卡点

### 卡点 1：`dotnet: command not found`

原因：没有安装 .NET SDK，或者安装后终端还没重新打开。

处理：

```bash
dotnet --version
```

如果还是不行，重新安装 SDK，然后重开终端。

### 卡点 2：`AddSwaggerGen` 找不到

原因：Swagger 包没有安装，或者模板没有自动带上。

处理：

```bash
dotnet add src/Api/Api.csproj package Swashbuckle.AspNetCore
dotnet build
```

### 卡点 3：测试里找不到 `Program`

原因：顶层语句生成的 `Program` 默认不方便测试项目访问。

处理：确认 `src/Api/Program.cs` 最底部有：

```csharp
public partial class Program
{
}
```

### 卡点 4：浏览器打不开 `/health`

原因通常是 API 没启动、端口号写错，或者访问了 https 但证书未信任。

处理：

- 先看 `dotnet run` 的终端是否还在运行。
- 复制终端显示的 `http://localhost:端口号`。
- 优先访问 `http://localhost:端口号/health`。

## 22. 建议提交

如果你使用 git，确认状态：

```bash
git status --short
```

如果 build 和 test 都通过，可以提交：

```bash
git add .
git commit -m "chore: create dotnet solution skeleton"
```

提交不是今天学习 .NET 的必要条件，但它能帮你保存一个干净的 Day 1 节点。
