using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.DTOs.Seguimiento;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    public class SeguimientosService : ISeguimientosService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUsuarioActualService _usuarioActual;

        public SeguimientosService(DataContext context, IMapper mapper, IUsuarioActualService usuarioActual)
        {
            _context = context;
            _mapper = mapper;
            _usuarioActual = usuarioActual;
        }

        public async Task<Response<SeguimientoDTO>> CreateAsync(CreateSeguimientoDTO dto)
        {
            try
            {
                OrdenServicio? orden = await _context.OrdenesServicio.AsNoTracking()
                                                                     .FirstOrDefaultAsync(o => o.Id == dto.OrdenServicioId);

                if (orden is null || !await PuedeVerAsync(orden))
                {
                    return Response<SeguimientoDTO>.Failure($"No existe orden con id {dto.OrdenServicioId}");
                }

                Seguimiento seguimiento = new Seguimiento
                {
                    Id = Guid.NewGuid(),
                    OrdenServicioId = orden.Id,
                    UsuarioId = _usuarioActual.UsuarioId!.Value,
                    Fecha = DateTime.Now,
                    Observacion = dto.Observacion.Trim(),
                };

                await _context.Seguimientos.AddAsync(seguimiento);
                await _context.SaveChangesAsync();

                await _context.Entry(seguimiento).Reference(s => s.Usuario).LoadAsync();

                return Response<SeguimientoDTO>.Success(_mapper.Map<SeguimientoDTO>(seguimiento), "Observación registrada con éxito");
            }
            catch (Exception ex)
            {
                return Response<SeguimientoDTO>.Failure(ex);
            }
        }

        public async Task<Response<List<SeguimientoDTO>>> GetByOrdenAsync(Guid ordenServicioId)
        {
            try
            {
                OrdenServicio? orden = await _context.OrdenesServicio.AsNoTracking()
                                                                     .FirstOrDefaultAsync(o => o.Id == ordenServicioId);

                if (orden is null || !await PuedeVerAsync(orden))
                {
                    return Response<List<SeguimientoDTO>>.Failure($"No existe orden con id {ordenServicioId}");
                }

                List<Seguimiento> seguimientos = await _context.Seguimientos.Include(s => s.Usuario)
                                                                            .Where(s => s.OrdenServicioId == ordenServicioId)
                                                                            .OrderByDescending(s => s.Fecha)
                                                                            .ToListAsync();

                return Response<List<SeguimientoDTO>>.Success(_mapper.Map<List<SeguimientoDTO>>(seguimientos));
            }
            catch (Exception ex)
            {
                return Response<List<SeguimientoDTO>>.Failure(ex);
            }
        }

        private async Task<bool> PuedeVerAsync(OrdenServicio orden)
        {
            if (await _usuarioActual.TienePermisoAsync(PermisosCatalogo.Ordenes.VerTodas))
            {
                return true;
            }

            return orden.TecnicoId is not null && orden.TecnicoId == _usuarioActual.UsuarioId;
        }
    }
}
