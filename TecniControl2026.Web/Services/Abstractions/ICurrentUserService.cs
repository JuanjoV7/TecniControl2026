namespace TecniControl2026.Web.Services.Abstractions
{
    // Data of the user who made the current HTTP request
    public interface ICurrentUserService
    {
        public Guid? UserId { get; }
        public Task<bool> HasPermissionAsync(string permission);
    }
}
