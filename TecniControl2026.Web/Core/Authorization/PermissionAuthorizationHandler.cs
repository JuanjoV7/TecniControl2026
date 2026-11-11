using Microsoft.AspNetCore.Authorization;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Core.Authorization
{
    // Queries the permissions in the database on every request, so that changes
    // to roles or the deactivation of a user take effect immediately.
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionsService _permissionsService;

        public PermissionAuthorizationHandler(IPermissionsService permissionsService)
        {
            _permissionsService = permissionsService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            Guid? userId = context.User.GetUserId();

            if (userId is null)
            {
                return;
            }

            if (await _permissionsService.UserHasPermissionAsync(userId.Value, requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }
}
