using Infrastructure.Persistence;
using Infrastructure.Persistence.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var repoRoot = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

var developmentSettings = Path.Combine(repoRoot, "src", "Api", "appsettings.Development.json");
if (!File.Exists(developmentSettings))
{
    Console.Error.WriteLine($"找不到配置文件: {developmentSettings}");
    return 1;
}

var configuration = new ConfigurationBuilder()
    .AddJsonFile(developmentSettings, optional: false, reloadOnChange: false)
    .Build();

var connectionString = configuration.GetSection("Postgres")["ConnectionString"];
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("appsettings.Development.json 里没有 Postgres:ConnectionString。");
    return 1;
}

var options = new DbContextOptionsBuilder<MetaServerDbContext>()
    .UseNpgsql(connectionString)
    .Options;

await using var dbContext = new MetaServerDbContext(options);

var usersAlreadyExist = await dbContext.Users.AnyAsync();
await DevelopmentSeedData.SeedAsync(dbContext);

if (usersAlreadyExist)
{
    Console.WriteLine("Seed skipped: users already exist.");
}
else
{
    Console.WriteLine("Seed inserted development data.");
}

return 0;
