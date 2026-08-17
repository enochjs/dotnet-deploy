using Domain.Entities.Pipelines;

namespace Domain.Entities;

public sealed class Deploy
{
    public Guid Id { get; set; }
    public Guid? PipelineId { get; set; }
    public Pipeline? Pipeline { get; set; }
    public string AppKey { get; set; } = string.Empty;
    public int? IterationId { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? CreatedByUserName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Env { get; set; } = string.Empty;
    public string? Version { get; set; }
    public bool? UseVpn { get; set; }
    public int? DeployType { get; set; }
    public string? SwimLane { get; set; }
    public string? IntegrationReleaseVersion { get; set; }
}