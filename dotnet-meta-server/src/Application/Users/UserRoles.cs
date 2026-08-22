namespace Application.Users;

public static class UserRoles
{
    public const int FrontEnd = 1;
    public const int BackEnd = 2;
    public const int Ued = 3;
    public const int Product = 4;
    public const int ProjectManager = 5;
    public const int Qa = 6;
    public const int Other = 99;
    
    public static bool IsValid(int? role)
    {
        return role is FrontEnd
            or BackEnd
            or Qa
            or Product
            or ProjectManager
            or Ued
            or Other;
    }
    
    public static string GetName(int role)
    {
        return role switch
        {
            FrontEnd => "前端",
            BackEnd => "后端",
            Ued => "UED",
            Product => "产品",
            ProjectManager => "项目经理",
            Qa => "测试",
            Other => "其他",
            _ => "未知",
        };
    }
}