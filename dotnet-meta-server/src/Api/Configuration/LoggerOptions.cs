namespace Api.Configuration;

public sealed class LoggerOptions
{
  public const string SectionName = "Logger";

  public bool IncludeRequestBody { get; init; }

  public bool IncludeResponseBody { get; init; }
}