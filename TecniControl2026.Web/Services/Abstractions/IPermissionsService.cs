namespace TecniControl2026.Web.Services.Abstractions
{
    public interface IPermissionsService
    {
        public Task<bool> UserHasPermissionAsync(Guid userId, string permission);
        public Task<HashSet<string>> GetUserPermissionsAsync(Guid userId);
    }
}
