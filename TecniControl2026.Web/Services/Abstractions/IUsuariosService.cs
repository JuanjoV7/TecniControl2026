using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.Usuario;

namespace TecniControl2026.Web.Services.Abstractions
{
    public interface IUsuariosService
    {
        public Task<Response<UsuarioDTO>> CreateAsync(CreateUsuarioDTO dto);
        public Task<Response<UsuarioDTO>> GetOneAsync(Guid id);
        public Task<Response<PaginationResponse<UsuarioDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<List<UsuarioDTO>>> GetTecnicosAsync();
        public Task<Response<UsuarioDTO>> UpdateAsync(UpdateUsuarioDTO dto);
        public Task<Response<object>> ToggleAsync(ToggleUsuarioStatusDTO dto);
    }
}
