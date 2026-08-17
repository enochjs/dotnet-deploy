using System.Text.Json;

namespace Domain.Entities.Pipelines;

public sealed class PipelineJob
{
    public Guid Id { get; set; }
    public Guid PipelineId { get; set; }
    public Pipeline Pipeline { get; set; } = null!;
    public int StageSeq { get; set; }
    public string JobKey { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? UnitKey { get; set; }
    public JsonDocument? Extra { get; set; }
}