namespace Domain.Entities;

public sealed class AppMonitor
{
    public Guid Id { get; set; }
    public string AppKey { get; set; } = string.Empty;
    public string Env { get; set; } = string.Empty;
    public string? Version { get; set; }
    public string? SourceUuid { get; set; }
    public string? TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Url { get; set; }
    public string? Browser { get; set; }
    public string? Message { get; set; }
    public string? Stack { get; set; }
    public int Status { get; set; }
    public string? Remark { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? ResolvedByUserId { get; set; }
    public string? ResolvedByUserName { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
}
