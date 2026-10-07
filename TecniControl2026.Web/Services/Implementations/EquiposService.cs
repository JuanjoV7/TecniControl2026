using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.DTOs.Equipo;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    public class EquiposService : CustomQueryableOperationsService, IEquiposService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public EquiposService(DataContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response<CreateEquipoDTO>> CreateAsync(CreateEquipoDTO dto)
        {
            Normalizar(dto);

            string? error = await ValidarAsync(dto, null);

            if (error is not null)
            {
                return Response<CreateEquipoDTO>.Failure(error);
            }

            Response<CreateEquipoDTO> response = await CreateAsync<CreateEquipoDTO, Equipo>(dto);

            if (response.IsSuccess)
            {
                response.Message = "Equipo registrado con éxito";
            }

            return response;
        }

        public async Task<Response<object>> DeleteAsync(Guid id)
        {
            if (!await _context.Equipos.AnyAsync(e => e.Id == id))
            {
                return Response<object>.Failure($"No existe equipo con id {id}");
            }

            bool tieneOrdenes = await _context.OrdenesServicio.AnyAsync(o => o.EquipoId == id);

            if (tieneOrdenes)
            {
                return Response<object>.Failure("No se puede eliminar el equipo porque tiene órdenes de servicio asociadas");
            }

            Response<object> response = await DeleteAsync<Equipo>(id);

            if (response.IsSuccess)
            {
                response.Message = "Equipo eliminado con éxito";
            }

            return response;
        }

        public async Task<Response<EquipoDTO>> GetOneAsync(Guid id)
        {
            return await GetOneAsync<EquipoDTO, Equipo>(id, _context.Equipos.Include(e => e.Cliente));
        }

        public async Task<Response<PaginationResponse<EquipoDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            IQueryable<Equipo> query = _context.Equipos.Include(e => e.Cliente).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                string filter = request.Filter.Trim().ToLower();

                query = query.Where(e => e.Marca.ToLower().Contains(filter)
                                      || e.Modelo.ToLower().Contains(filter)
                                      || (e.Serial != null && e.Serial.ToLower().Contains(filter))
                                      || e.Cliente.Nombre.ToLower().Contains(filter)
                                      || e.Cliente.Documento.ToLower().Contains(filter));
            }

            query = query.OrderBy(e => e.Cliente.Nombre)
                         .ThenBy(e => e.Marca)
                         .ThenBy(e => e.Modelo);

            return await GetPagedListAsync<EquipoDTO, Equipo>(request, query);
        }

        public async Task<Response<List<EquipoDTO>>> GetByClienteAsync(Guid clienteId)
        {
            try
            {
                List<Equipo> equipos = await _context.Equipos.Include(e => e.Cliente)
                                                             .Where(e => e.ClienteId == clienteId)
                                                             .OrderBy(e => e.Marca)
                                                             .ThenBy(e => e.Modelo)
                                                             .ToListAsync();

                return Response<List<EquipoDTO>>.Success(_mapper.Map<List<EquipoDTO>>(equipos));
            }
            catch (Exception ex)
            {
                return Response<List<EquipoDTO>>.Failure(ex);
            }
        }

        public async Task<Response<EquipoDTO>> UpdateAsync(UpdateEquipoDTO dto)
        {
            Normalizar(dto);

            if (!await _context.Equipos.AnyAsync(e => e.Id == dto.Id))
            {
                return Response<EquipoDTO>.Failure($"No existe equipo con id {dto.Id}");
            }

            string? error = await ValidarAsync(dto, dto.Id);

            if (error is not null)
            {
                return Response<EquipoDTO>.Failure(error);
            }

            Response<UpdateEquipoDTO> result = await UpdateAsync<UpdateEquipoDTO, Equipo>(dto, dto.Id);

            if (!result.IsSuccess)
            {
                return Response<EquipoDTO>.Failure(result.Message ?? "No fue posible actualizar el equipo", result.Errors);
            }

            Response<EquipoDTO> response = await GetOneAsync(dto.Id);
            response.Message = "Equipo actualizado con éxito";

            return response;
        }

        private static void Normalizar(CreateEquipoDTO dto)
        {
            dto.Marca = dto.Marca.Trim();
            dto.Modelo = dto.Modelo.Trim();
            dto.Serial = string.IsNullOrWhiteSpace(dto.Serial) ? null : dto.Serial.Trim();
        }

        private async Task<string?> ValidarAsync(CreateEquipoDTO dto, Guid? equipoId)
        {
            Cliente? cliente = await _context.Clientes.AsNoTracking()
                                                      .FirstOrDefaultAsync(c => c.Id == dto.ClienteId);

            if (cliente is null)
            {
                return "El cliente seleccionado no existe";
            }

            if (!cliente.Activo)
            {
                return "El cliente seleccionado está inactivo";
            }

            if (dto.Serial is not null
                && await _context.Equipos.AnyAsync(e => e.Serial == dto.Serial && e.Id != equipoId))
            {
                return "Ya existe un equipo con ese serial";
            }

            return null;
        }
    }
}
