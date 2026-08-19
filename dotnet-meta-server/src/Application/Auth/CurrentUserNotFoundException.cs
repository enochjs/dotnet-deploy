namespace Application.Auth;

public sealed class CurrentUserNotFoundException() : Exception("当前用户不存在")
{

}