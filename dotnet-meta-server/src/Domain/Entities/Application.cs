using System.Text.Json;
using Domain.Entities.Pipelines.Templates;

namespace Domain.Entities;

public sealed class Application {

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