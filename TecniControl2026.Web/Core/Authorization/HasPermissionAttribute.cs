using Microsoft.AspNetCore.Authorization;

namespace TecniControl2026.Web.Core.Authorization
{
    // Usage: [HasPermission(PermissionsCatalog.Customers.Create)]
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public const string PolicyPrefix = "Permission:";

        public HasPermissionAttribute(string permission)
        {
            Policy = PolicyPrefix + permission;
        }
    }
}
