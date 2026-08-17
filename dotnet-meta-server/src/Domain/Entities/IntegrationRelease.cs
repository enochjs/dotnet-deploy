namespace Domain.Entities;

public sealed class IntegrationRelease
{
    public int Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Branch { get; set; }
    public string? Remark { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? CreatedByUserName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<IntegrationReleaseApp> ReleaseApps { get; } = [];
}