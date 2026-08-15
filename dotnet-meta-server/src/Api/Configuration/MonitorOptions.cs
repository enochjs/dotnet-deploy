namespace Api.Configuration;

public sealed class MonitorOptions
{
  public const string SectionName = "Monitor";

  public bool Enabled { get; init; } = true;

  public int SlowRequestThresholdMs { get; init; } = 1000;
}