namespace Domain.Entities;

public sealed class IntegrationReleaseApp
{
    public int Id { get; set; }
    public int IntegrationReleaseId { get; set; }
    public IntegrationRelease IntegrationRelease { get; set; } = null!;
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    public int SubApplicationId { get; set; }
    public SubApplication SubApplication { get; set; } = null!;
    public string AppKey { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string SubApplicationName { get; set; } = string.Empty;
    public int IterationId { get; set; }
    public Iteration Iteration { get; set; } = null!;
}