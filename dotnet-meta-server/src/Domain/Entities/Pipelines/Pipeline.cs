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