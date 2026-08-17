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