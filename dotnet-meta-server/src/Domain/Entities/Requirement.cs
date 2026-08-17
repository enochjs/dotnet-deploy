namespace Domain.Entities;

public sealed class Requirement
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
    public string DocumentUrl { get; set; } = string.Empty;
    public int? Priority { get; set; }
    public string? Remark { get; set; }
    public DateTimeOffset? OnlineAt { get; set; }
    public DateTimeOffset? SubmittedTestAt { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? CreatedByUserName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string UpdatedByUserId { get; set; } = string.Empty;
    public string? UpdatedByUserName { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<User> Developers { get; } = [];
    public ICollection<User> Followers { get; } = [];
    public ICollection<Iteration> Iterations { get; } = [];
}