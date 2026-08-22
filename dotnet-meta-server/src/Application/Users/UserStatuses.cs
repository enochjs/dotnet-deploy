namespace Application.Users;

public static class UserStatuses
{
    public const int Enabled = 1;
    public const int Disabled = 2;

    public static bool IsValid(int? status)
    {
        return status is Enabled or Disabled;
    }
}