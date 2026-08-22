namespace Application.Common;

public sealed class RequestValidationException(IReadOnlyDictionary<string, string[]> errors) : Exception("请求参数不正确")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}