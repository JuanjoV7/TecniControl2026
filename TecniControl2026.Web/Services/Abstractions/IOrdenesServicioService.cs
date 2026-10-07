using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.OrdenServicio;

namespace TecniControl2026.Web.Services.Abstractions
{
    public interface IOrdenesServicioService
    {
        public Task<Response<OrdenServicioDTO>> CreateAsync(CreateOrdenServicioDTO dto);
        public Task<Response<OrdenServicioDetalleDTO>> GetOneAsync(Guid id);
        public Task<Response<PaginationResponse<OrdenServicioDTO>>> GetPaginationAsync(OrdenesPaginationRequest request);
        public Task<Response<List<OrdenServicioDTO>>> GetHistorialEquipoAsync(Guid equipoId);
        public Task<Response<object>> AsignarTecnicoAsync(AsignarTecnicoDTO dto);
        public Task<Response<object>> RegistrarAtencionAsync(RegistrarAtencionDTO dto);
        public Task<Response<object>> EntregarAsync(EntregarOrdenDTO dto);
        public Task<Response<object>> CancelarAsync(CancelarOrdenDTO dto);
    }
}
