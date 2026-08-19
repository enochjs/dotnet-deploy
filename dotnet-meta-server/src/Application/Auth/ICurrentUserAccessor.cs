namespace Application.Auth;

public interface ICurrentUserAccessor
{
  CurrentUser GetRequiredCurrentUser();
}