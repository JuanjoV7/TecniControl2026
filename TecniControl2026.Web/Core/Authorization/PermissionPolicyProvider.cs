using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace TecniControl2026.Web.Core.Authorization
{
    // Creates a policy on the fly for each permission ("Permission:customers.create"),
    // so there is no need to register a policy for each catalog code.
    public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(HasPermissionAttribute.PolicyPrefix, StringComparison.Ordinal))
            {
                string permission = policyName.Substring(HasPermissionAttribute.PolicyPrefix.Length);

                return new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermissionRequirement(permission))
                    .Build();
            }

            return await base.GetPolicyAsync(policyName);
        }
    }
}
