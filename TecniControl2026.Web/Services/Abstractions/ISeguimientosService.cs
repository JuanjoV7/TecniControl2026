using TecniControl2026.Web.Core;
using TecniControl2026.Web.DTOs.Seguimiento;

namespace TecniControl2026.Web.Services.Abstractions
{
    public interface ISeguimientosService
    {
        public Task<Response<SeguimientoDTO>> CreateAsync(CreateSeguimientoDTO dto);
        public Task<Response<List<SeguimientoDTO>>> GetByOrdenAsync(Guid ordenServicioId);
    }
}
