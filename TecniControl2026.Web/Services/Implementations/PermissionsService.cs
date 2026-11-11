using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    // Scoped: each user's permissions are queried only once per request
    public class PermissionsService : IPermissionsService
    {
        private readonly DataContext _context;
        private readonly Dictionary<Guid, HashSet<string>> _cache = new();

        public PermissionsService(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> UserHasPermissionAsync(Guid userId, string permission)
        {
            HashSet<string> permissions = await GetUserPermissionsAsync(userId);

            return permissions.Contains(permission);
        }

        public async Task<HashSet<string>> GetUserPermissionsAsync(Guid userId)
        {
            if (_cache.TryGetValue(userId, out HashSet<string>? permissions))
            {
                return permissions;
            }

            // A deactivated user has no permissions
            List<string> codes = await _context.Users.Where(u => u.Id == userId && u.IsActive)
                                                       .SelectMany(u => u.Role.RolePermissions)
                                                       .Select(rp => rp.Permission.Code)
                                                       .ToListAsync();

            permissions = new HashSet<string>(codes);
            _cache[userId] = permissions;

            return permissions;
        }
    }
}
