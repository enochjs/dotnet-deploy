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