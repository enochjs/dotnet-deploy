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