using Microsoft.AspNetCore.Authorization;

namespace MockSchoolManagement.Security
{
    /// <summary>
    /// 管理Admin角色与声明的需求
    /// </summary>
    public class ManageAdminRolesAndClaimsRequirement:IAuthorizationRequirement
    {
    }
}
