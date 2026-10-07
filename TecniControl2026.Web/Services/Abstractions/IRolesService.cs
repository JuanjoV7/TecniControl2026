using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.Rol;

namespace TecniControl2026.Web.Services.Abstractions
{
    public interface IRolesService
    {
        public Task<Response<RolDTO>> CreateAsync(CreateRolDTO dto);
        public Task<Response<RolDTO>> GetOneAsync(Guid id);
        public Task<Response<PaginationResponse<RolDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<List<RolDTO>>> GetAllAsync();
        public Task<Response<List<PermisoDTO>>> GetPermisosAsync();
        public Task<Response<RolDTO>> UpdateAsync(UpdateRolDTO dto);
        public Task<Response<object>> DeleteAsync(Guid id);
    }
}
