namespace Domain.Entities;

public sealed class Iteration
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    public int? SubApplicationId { get; set; }
    public SubApplication? SubApplication { get; set; }
    public int? IntegrationReleaseId { get; set; }
    public IntegrationRelease? IntegrationRelease { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string? SubApplicationName { get; set; }
    public string Branch { get; set; } = string.Empty;
    public string? OriginalCommit { get; set; }
    public int Status { get; set; }
    public string? Remark { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? CreatedByUserName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string UpdatedByUserId { get; set; } = string.Empty;
    public string? UpdatedByUserName { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<Requirement> Requirements { get; } = [];
}