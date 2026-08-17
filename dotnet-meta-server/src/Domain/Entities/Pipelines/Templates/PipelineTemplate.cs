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