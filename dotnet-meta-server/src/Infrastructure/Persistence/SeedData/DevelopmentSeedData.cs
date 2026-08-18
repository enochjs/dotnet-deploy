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
            GitId = 1001,
            RegistryKey = "fe",
            GitName = "meta-web",
            GitRepo = "git@example.com:devops/meta-web.git",
            GitNamespaceId = 10,
            TriggerToken = "trigger-token-for-test",
            OwnerUserId = owner.UserId,
            OwnerName = owner.Name,
            Status = 1,
            Remark = "Seed application for integration tests.",
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
        subApplication.PipelineTemplates.Add(template);

        dbContext.Users.AddRange(owner, developer);
        dbContext.Applications.Add(application);
        dbContext.Requirements.Add(requirement);
        dbContext.Iterations.Add(iteration);
        dbContext.PipelineTemplates.Add(template);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}