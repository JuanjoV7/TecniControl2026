using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.Data.Enums;
using TecniControl2026.Web.DTOs.OrdenServicio;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    public class OrdenesServicioService : CustomQueryableOperationsService, IOrdenesServicioService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUsuarioActualService _usuarioActual;
        private readonly IPermisosService _permisosService;

        public OrdenesServicioService(DataContext context, IMapper mapper, IUsuarioActualService usuarioActual, IPermisosService permisosService)
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _usuarioActual = usuarioActual;
            _permisosService = permisosService;
        }

        public async Task<Response<OrdenServicioDTO>> CreateAsync(CreateOrdenServicioDTO dto)
        {
            try
            {
                Guid? usuarioId = _usuarioActual.UsuarioId;

                if (usuarioId is null)
                {
                    return Response<OrdenServicioDTO>.Failure("No hay un usuario autenticado");
                }

                Equipo? equipo = await _context.Equipos.Include(e => e.Cliente)
                                                       .FirstOrDefaultAsync(e => e.Id == dto.EquipoId);

                if (equipo is null)
                {
                    return Response<OrdenServicioDTO>.Failure("El equipo seleccionado no existe");
                }

                if (!equipo.Cliente.Activo)
                {
                    return Response<OrdenServicioDTO>.Failure("El cliente del equipo está inactivo");
                }

                bool tieneOrdenAbierta = await _context.OrdenesServicio.AnyAsync(o => o.EquipoId == dto.EquipoId
                                                                                   && o.Estado != EstadoOrden.Entregada
                                                                                   && o.Estado != EstadoOrden.Cancelada);

                if (tieneOrdenAbierta)
                {
                    return Response<OrdenServicioDTO>.Failure("El equipo ya tiene una orden de servicio abierta");
                }

                Usuario? tecnico = null;

                if (dto.TecnicoId is not null)
                {
                    Response<Usuario> tecnicoResponse = await ValidarTecnicoAsync(dto.TecnicoId.Value);

                    if (!tecnicoResponse.IsSuccess)
                    {
                        return Response<OrdenServicioDTO>.Failure(tecnicoResponse.Message!);
                    }

                    tecnico = tecnicoResponse.Result;
                }

                DateTime ahora = DateTime.Now;

                OrdenServicio orden = new OrdenServicio
                {
                    Id = Guid.NewGuid(),
                    EquipoId = equipo.Id,
                    FechaRecepcion = ahora,
                    RecibidaPorId = usuarioId.Value,
                    FallaReportada = dto.FallaReportada.Trim(),
                    Accesorios = string.IsNullOrWhiteSpace(dto.Accesorios) ? null : dto.Accesorios.Trim(),
                    TecnicoId = tecnico?.Id,
                    Estado = EstadoOrden.Recibida,
                };

                string observacion = "Equipo recibido en el taller.";

                if (tecnico is not null)
                {
                    observacion += $" Técnico asignado: {tecnico.NombreCompleto}.";
                }

                orden.Seguimientos.Add(NuevoSeguimiento(usuarioId.Value, ahora, observacion, null, EstadoOrden.Recibida));

                await _context.OrdenesServicio.AddAsync(orden);
                await _context.SaveChangesAsync();

                OrdenServicio creada = await QueryConDetalle().FirstAsync(o => o.Id == orden.Id);

                return Response<OrdenServicioDTO>.Success(_mapper.Map<OrdenServicioDTO>(creada), $"Orden N° {creada.Numero} registrada con éxito");
            }
            catch (Exception ex)
            {
                return Response<OrdenServicioDTO>.Failure(ex);
            }
        }

        public async Task<Response<OrdenServicioDetalleDTO>> GetOneAsync(Guid id)
        {
            try
            {
                OrdenServicio? orden = await QueryConDetalle().Include(o => o.Seguimientos)
                                                              .ThenInclude(s => s.Usuario)
                                                              .AsSplitQuery()
                                                              .FirstOrDefaultAsync(o => o.Id == id);

                if (orden is null || !await PuedeVerAsync(orden))
                {
                    return Response<OrdenServicioDetalleDTO>.Failure($"No existe orden con id {id}");
                }

                OrdenServicioDetalleDTO dto = _mapper.Map<OrdenServicioDetalleDTO>(orden);
                dto.Seguimientos = dto.Seguimientos.OrderByDescending(s => s.Fecha).ToList();

                return Response<OrdenServicioDetalleDTO>.Success(dto, "Orden obtenida con éxito");
            }
            catch (Exception ex)
            {
                return Response<OrdenServicioDetalleDTO>.Failure(ex);
            }
        }

        public async Task<Response<PaginationResponse<OrdenServicioDTO>>> GetPaginationAsync(OrdenesPaginationRequest request)
        {
            Response<IQueryable<OrdenServicio>> visibles = await QueryVisibles();

            if (!visibles.IsSuccess)
            {
                return Response<PaginationResponse<OrdenServicioDTO>>.Failure(visibles.Message!);
            }

            IQueryable<OrdenServicio> query = visibles.Result!;

            if (request.Estado is not null)
            {
                query = query.Where(o => o.Estado == request.Estado);
            }

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                string filter = request.Filter.Trim().ToLower();

                if (int.TryParse(filter, out int numero))
                {
                    query = query.Where(o => o.Numero == numero);
                }
                else
                {
                    query = query.Where(o => o.Equipo.Cliente.Nombre.ToLower().Contains(filter)
                                          || o.Equipo.Cliente.Documento.ToLower().Contains(filter)
                                          || o.Equipo.Marca.ToLower().Contains(filter)
                                          || o.Equipo.Modelo.ToLower().Contains(filter)
                                          || (o.Equipo.Serial != null && o.Equipo.Serial.ToLower().Contains(filter)));
                }
            }

            query = query.OrderByDescending(o => o.FechaRecepcion);

            return await GetPagedListAsync<OrdenServicioDTO, OrdenServicio>(request, query);
        }

        public async Task<Response<List<OrdenServicioDTO>>> GetHistorialEquipoAsync(Guid equipoId)
        {
            try
            {
                Response<IQueryable<OrdenServicio>> visibles = await QueryVisibles();

                if (!visibles.IsSuccess)
                {
                    return Response<List<OrdenServicioDTO>>.Failure(visibles.Message!);
                }

                List<OrdenServicio> ordenes = await visibles.Result!.Where(o => o.EquipoId == equipoId)
                                                                    .OrderByDescending(o => o.FechaRecepcion)
                                                                    .ToListAsync();

                return Response<List<OrdenServicioDTO>>.Success(_mapper.Map<List<OrdenServicioDTO>>(ordenes));
            }
            catch (Exception ex)
            {
                return Response<List<OrdenServicioDTO>>.Failure(ex);
            }
        }

        public async Task<Response<object>> AsignarTecnicoAsync(AsignarTecnicoDTO dto)
        {
            try
            {
                OrdenServicio? orden = await _context.OrdenesServicio.Include(o => o.Tecnico)
                                                                     .FirstOrDefaultAsync(o => o.Id == dto.OrdenServicioId);

                if (orden is null)
                {
                    return Response<object>.Failure($"No existe orden con id {dto.OrdenServicioId}");
                }

                if (OrdenEstadoReglas.EsFinal(orden.Estado))
                {
                    return Response<object>.Failure("No se puede asignar técnico a una orden entregada o cancelada");
                }

                if (orden.TecnicoId == dto.TecnicoId)
                {
                    return Response<object>.Failure("El técnico ya está asignado a esta orden");
                }

                Response<Usuario> tecnicoResponse = await ValidarTecnicoAsync(dto.TecnicoId);

                if (!tecnicoResponse.IsSuccess)
                {
                    return Response<object>.Failure(tecnicoResponse.Message!);
                }

                Usuario tecnico = tecnicoResponse.Result!;

                string observacion = orden.Tecnico is null
                    ? $"Técnico asignado: {tecnico.NombreCompleto}."
                    : $"Técnico cambiado de {orden.Tecnico.NombreCompleto} a {tecnico.NombreCompleto}.";

                orden.TecnicoId = tecnico.Id;
                orden.Seguimientos.Add(NuevoSeguimiento(_usuarioActual.UsuarioId!.Value, DateTime.Now, observacion, null, null));

                await _context.SaveChangesAsync();

                return Response<object>.Success("Técnico asignado con éxito");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        public async Task<Response<object>> RegistrarAtencionAsync(RegistrarAtencionDTO dto)
        {
            try
            {
                OrdenServicio? orden = await _context.OrdenesServicio.FirstOrDefaultAsync(o => o.Id == dto.OrdenServicioId);

                if (orden is null)
                {
                    return Response<object>.Failure($"No existe orden con id {dto.OrdenServicioId}");
                }

                if (OrdenEstadoReglas.EsFinal(orden.Estado))
                {
                    return Response<object>.Failure("La orden ya fue entregada o cancelada");
                }

                if (orden.TecnicoId is null)
                {
                    return Response<object>.Failure("La orden no tiene un técnico asignado");
                }

                // Solo el técnico asignado, o quien puede ver todas las órdenes, registra la atención
                if (orden.TecnicoId != _usuarioActual.UsuarioId
                    && !await _usuarioActual.TienePermisoAsync(PermisosCatalogo.Ordenes.VerTodas))
                {
                    return Response<object>.Failure("La orden no está asignada a usted");
                }

                if (!OrdenEstadoReglas.EstadosDeAtencion.Contains(dto.Estado))
                {
                    return Response<object>.Failure($"El estado {OrdenEstadoReglas.Nombre(dto.Estado)} no se puede asignar desde la atención técnica");
                }

                EstadoOrden estadoAnterior = orden.Estado;
                bool cambiaEstado = estadoAnterior != dto.Estado;

                if (cambiaEstado && !OrdenEstadoReglas.PuedeCambiar(estadoAnterior, dto.Estado))
                {
                    return Response<object>.Failure($"No se puede pasar de {OrdenEstadoReglas.Nombre(estadoAnterior)} a {OrdenEstadoReglas.Nombre(dto.Estado)}");
                }

                string? diagnostico = string.IsNullOrWhiteSpace(dto.Diagnostico) ? null : dto.Diagnostico.Trim();
                string? trabajo = string.IsNullOrWhiteSpace(dto.TrabajoRealizado) ? null : dto.TrabajoRealizado.Trim();

                if ((dto.Estado is EstadoOrden.EnReparacion or EstadoOrden.ListaParaEntrega) && diagnostico is null)
                {
                    return Response<object>.Failure("Debe registrar el diagnóstico antes de avanzar la orden");
                }

                if (dto.Estado == EstadoOrden.ListaParaEntrega && trabajo is null)
                {
                    return Response<object>.Failure("Debe registrar el trabajo realizado antes de marcar la orden como lista para entrega");
                }

                orden.Diagnostico = diagnostico;
                orden.TrabajoRealizado = trabajo;
                orden.Estado = dto.Estado;

                string observacion = string.IsNullOrWhiteSpace(dto.Observacion)
                    ? (cambiaEstado ? $"Estado actualizado a {OrdenEstadoReglas.Nombre(dto.Estado)}." : "Atención técnica actualizada.")
                    : dto.Observacion.Trim();

                orden.Seguimientos.Add(NuevoSeguimiento(_usuarioActual.UsuarioId!.Value,
                                                        DateTime.Now,
                                                        observacion,
                                                        cambiaEstado ? estadoAnterior : null,
                                                        cambiaEstado ? dto.Estado : null));

                await _context.SaveChangesAsync();

                return Response<object>.Success("Atención registrada con éxito");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        public async Task<Response<object>> EntregarAsync(EntregarOrdenDTO dto)
        {
            try
            {
                OrdenServicio? orden = await _context.OrdenesServicio.FirstOrDefaultAsync(o => o.Id == dto.OrdenServicioId);

                if (orden is null)
                {
                    return Response<object>.Failure($"No existe orden con id {dto.OrdenServicioId}");
                }

                if (orden.Estado != EstadoOrden.ListaParaEntrega)
                {
                    return Response<object>.Failure("Solo se pueden entregar órdenes que estén listas para entrega");
                }

                DateTime ahora = DateTime.Now;

                orden.Estado = EstadoOrden.Entregada;
                orden.FechaEntrega = ahora;

                string observacion = string.IsNullOrWhiteSpace(dto.Observacion)
                    ? "Equipo entregado al cliente."
                    : dto.Observacion.Trim();

                orden.Seguimientos.Add(NuevoSeguimiento(_usuarioActual.UsuarioId!.Value, ahora, observacion,
                                                        EstadoOrden.ListaParaEntrega, EstadoOrden.Entregada));

                await _context.SaveChangesAsync();

                return Response<object>.Success($"Orden N° {orden.Numero} entregada con éxito");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        public async Task<Response<object>> CancelarAsync(CancelarOrdenDTO dto)
        {
            try
            {
                OrdenServicio? orden = await _context.OrdenesServicio.FirstOrDefaultAsync(o => o.Id == dto.OrdenServicioId);

                if (orden is null)
                {
                    return Response<object>.Failure($"No existe orden con id {dto.OrdenServicioId}");
                }

                if (OrdenEstadoReglas.EsFinal(orden.Estado))
                {
                    return Response<object>.Failure("La orden ya fue entregada o cancelada");
                }

                DateTime ahora = DateTime.Now;
                EstadoOrden estadoAnterior = orden.Estado;
                string motivo = dto.Motivo.Trim();

                orden.Estado = EstadoOrden.Cancelada;
                orden.FechaCancelacion = ahora;
                orden.MotivoCancelacion = motivo;

                orden.Seguimientos.Add(NuevoSeguimiento(_usuarioActual.UsuarioId!.Value, ahora, $"Orden cancelada. Motivo: {motivo}",
                                                        estadoAnterior, EstadoOrden.Cancelada));

                await _context.SaveChangesAsync();

                return Response<object>.Success($"Orden N° {orden.Numero} cancelada");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        private IQueryable<OrdenServicio> QueryConDetalle()
        {
            return _context.OrdenesServicio.Include(o => o.Equipo)
                                           .ThenInclude(e => e.Cliente)
                                           .Include(o => o.Tecnico)
                                           .Include(o => o.RecibidaPor);
        }

        // Quien tiene Ordenes.VerTodas ve todas; quien tiene Ordenes.VerAsignadas solo las suyas
        private async Task<Response<IQueryable<OrdenServicio>>> QueryVisibles()
        {
            IQueryable<OrdenServicio> query = QueryConDetalle();

            if (await _usuarioActual.TienePermisoAsync(PermisosCatalogo.Ordenes.VerTodas))
            {
                return Response<IQueryable<OrdenServicio>>.Success(query);
            }

            if (await _usuarioActual.TienePermisoAsync(PermisosCatalogo.Ordenes.VerAsignadas))
            {
                Guid usuarioId = _usuarioActual.UsuarioId!.Value;

                return Response<IQueryable<OrdenServicio>>.Success(query.Where(o => o.TecnicoId == usuarioId));
            }

            return Response<IQueryable<OrdenServicio>>.Failure("No tiene permiso para consultar órdenes");
        }

        private async Task<bool> PuedeVerAsync(OrdenServicio orden)
        {
            if (await _usuarioActual.TienePermisoAsync(PermisosCatalogo.Ordenes.VerTodas))
            {
                return true;
            }

            return orden.TecnicoId is not null
                && orden.TecnicoId == _usuarioActual.UsuarioId
                && await _usuarioActual.TienePermisoAsync(PermisosCatalogo.Ordenes.VerAsignadas);
        }

        private async Task<Response<Usuario>> ValidarTecnicoAsync(Guid tecnicoId)
        {
            Usuario? tecnico = await _context.Users.FirstOrDefaultAsync(u => u.Id == tecnicoId);

            if (tecnico is null || !tecnico.Activo)
            {
                return Response<Usuario>.Failure("El técnico seleccionado no existe o está inactivo");
            }

            if (!await _permisosService.UsuarioTienePermisoAsync(tecnico.Id, PermisosCatalogo.Ordenes.Atender))
            {
                return Response<Usuario>.Failure("El usuario seleccionado no tiene permiso para atender órdenes");
            }

            return Response<Usuario>.Success(tecnico);
        }

        private static Seguimiento NuevoSeguimiento(Guid usuarioId, DateTime fecha, string observacion, EstadoOrden? anterior, EstadoOrden? nuevo)
        {
            return new Seguimiento
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                Fecha = fecha,
                Observacion = observacion,
                EstadoAnterior = anterior,
                EstadoNuevo = nuevo,
            };
        }
    }
}
