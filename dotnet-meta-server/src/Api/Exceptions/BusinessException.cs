namespace Api.Exceptions;

public sealed class BusinessException: Exception {

  public string Code { get; }

  public BusinessException(string code, string message): base(message) {
    Code = code;
  }

}