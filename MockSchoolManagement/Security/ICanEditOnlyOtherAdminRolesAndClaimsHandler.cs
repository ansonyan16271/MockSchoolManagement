using Microsoft.AspNetCore.Authorization;

namespace MockSchoolManagement.Security
{
    public interface ICanEditOnlyOtherAdminRolesAndClaimsHandler
    {
        Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageAdminRolesAndClaimsRequirement requirement);
    }
}