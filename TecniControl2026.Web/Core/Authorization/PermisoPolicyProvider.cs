using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace TecniControl2026.Web.Core.Authorization
{
    // Crea al vuelo una política por cada permiso ("Permiso:Clientes.Crear"),
    // así no hay que registrar una política por cada código del catálogo.
    public class PermisoPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermisoPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(TienePermisoAttribute.PolicyPrefix, StringComparison.Ordinal))
            {
                string permiso = policyName.Substring(TienePermisoAttribute.PolicyPrefix.Length);

                return new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermisoRequirement(permiso))
                    .Build();
            }

            return await base.GetPolicyAsync(policyName);
        }
    }
}
