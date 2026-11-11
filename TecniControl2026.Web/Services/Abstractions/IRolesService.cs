using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.Role;

namespace TecniControl2026.Web.Services.Abstractions
{
    public interface IRolesService
    {
        public Task<Response<RoleDTO>> CreateAsync(CreateRoleDTO dto);
        public Task<Response<RoleDTO>> GetOneAsync(Guid id);
        public Task<Response<PaginationResponse<RoleDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<List<RoleDTO>>> GetAllAsync();
        public Task<Response<List<PermissionDTO>>> GetPermissionsAsync();
        public Task<Response<RoleDTO>> UpdateAsync(UpdateRoleDTO dto);
        public Task<Response<object>> DeleteAsync(Guid id);
    }
}
