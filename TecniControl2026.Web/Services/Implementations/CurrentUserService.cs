using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPermissionsService _permissionsService;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, IPermissionsService permissionsService)
        {
            _httpContextAccessor = httpContextAccessor;
            _permissionsService = permissionsService;
        }

        public Guid? UserId => _httpContextAccessor.HttpContext?.User.GetUserId();

        public async Task<bool> HasPermissionAsync(string permission)
        {
            Guid? userId = UserId;

            return userId is not null && await _permissionsService.UserHasPermissionAsync(userId.Value, permission);
        }
    }
}
