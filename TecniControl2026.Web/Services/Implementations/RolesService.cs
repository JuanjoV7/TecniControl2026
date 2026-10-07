using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.DTOs.Rol;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    public class RolesService : IRolesService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public RolesService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response<RolDTO>> CreateAsync(CreateRolDTO dto)
        {
            try
            {
                string nombre = dto.Nombre.Trim();

                if (await _context.Roles.AnyAsync(r => r.Nombre == nombre))
                {
                    return Response<RolDTO>.Failure("Ya existe un rol con ese nombre");
                }

                List<Guid> permisoIds = await PermisosValidosAsync(dto.PermisoIds);

                Rol rol = new Rol
                {
                    Id = Guid.NewGuid(),
                    Nombre = nombre,
                    Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                };

                foreach (Guid permisoId in permisoIds)
                {
                    rol.RolPermisos.Add(new RolPermiso { RolId = rol.Id, PermisoId = permisoId });
                }

                await _context.Roles.AddAsync(rol);
                await _context.SaveChangesAsync();

                Response<RolDTO> response = await GetOneAsync(rol.Id);
                response.Message = "Rol creado con éxito";

                return response;
            }
            catch (Exception ex)
            {
                return Response<RolDTO>.Failure(ex);
            }
        }

        public async Task<Response<RolDTO>> GetOneAsync(Guid id)
        {
            try
            {
                RolDTO? rol = await Proyectar(_context.Roles.Where(r => r.Id == id)).FirstOrDefaultAsync();

                if (rol is null)
                {
                    return Response<RolDTO>.Failure($"No existe rol con id {id}");
                }

                return Response<RolDTO>.Success(rol, "Rol obtenido con éxito");
            }
            catch (Exception ex)
            {
                return Response<RolDTO>.Failure(ex);
            }
        }

        public async Task<Response<PaginationResponse<RolDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            try
            {
                IQueryable<Rol> query = _context.Roles.AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.Filter))
                {
                    string filter = request.Filter.Trim().ToLower();

                    query = query.Where(r => r.Nombre.ToLower().Contains(filter)
                                          || (r.Descripcion != null && r.Descripcion.ToLower().Contains(filter)));
                }

                PagedList<RolDTO> list = await PagedList<RolDTO>.ToPagedListAsync(Proyectar(query.OrderBy(r => r.Nombre)), request);

                PaginationResponse<RolDTO> dto = new PaginationResponse<RolDTO>
                {
                    List = list,
                    CurrentPage = list.CurrentPage,
                    TotalPages = list.TotalPages,
                    RecordsPerPage = list.RecordsPerPage,
                    TotalCount = list.TotalCount,
                    Filter = request.Filter,
                };

                return Response<PaginationResponse<RolDTO>>.Success(dto, "Registros obtenidos con éxito");
            }
            catch (Exception ex)
            {
                return Response<PaginationResponse<RolDTO>>.Failure(ex);
            }
        }

        public async Task<Response<List<RolDTO>>> GetAllAsync()
        {
            try
            {
                List<RolDTO> roles = await Proyectar(_context.Roles.OrderBy(r => r.Nombre)).ToListAsync();

                return Response<List<RolDTO>>.Success(roles);
            }
            catch (Exception ex)
            {
                return Response<List<RolDTO>>.Failure(ex);
            }
        }

        public async Task<Response<List<PermisoDTO>>> GetPermisosAsync()
        {
            try
            {
                List<Permiso> permisos = await _context.Permisos.OrderBy(p => p.Modulo)
                                                                .ThenBy(p => p.Codigo)
                                                                .ToListAsync();

                return Response<List<PermisoDTO>>.Success(_mapper.Map<List<PermisoDTO>>(permisos));
            }
            catch (Exception ex)
            {
                return Response<List<PermisoDTO>>.Failure(ex);
            }
        }

        public async Task<Response<RolDTO>> UpdateAsync(UpdateRolDTO dto)
        {
            try
            {
                Rol? rol = await _context.Roles.Include(r => r.RolPermisos)
                                               .FirstOrDefaultAsync(r => r.Id == dto.Id);

                if (rol is null)
                {
                    return Response<RolDTO>.Failure($"No existe rol con id {dto.Id}");
                }

                // El administrador siempre conserva todos los permisos (ver SeedDb)
                if (rol.Nombre == RolesBase.Administrador)
                {
                    return Response<RolDTO>.Failure("El rol Administrador no se puede modificar");
                }

                string nombre = dto.Nombre.Trim();

                if (await _context.Roles.AnyAsync(r => r.Nombre == nombre && r.Id != dto.Id))
                {
                    return Response<RolDTO>.Failure("Ya existe otro rol con ese nombre");
                }

                rol.Nombre = nombre;
                rol.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();

                List<Guid> permisoIds = await PermisosValidosAsync(dto.PermisoIds);

                List<RolPermiso> retirados = rol.RolPermisos.Where(rp => !permisoIds.Contains(rp.PermisoId)).ToList();
                _context.RolPermisos.RemoveRange(retirados);

                IEnumerable<Guid> nuevos = permisoIds.Where(id => !rol.RolPermisos.Any(rp => rp.PermisoId == id));

                foreach (Guid permisoId in nuevos)
                {
                    rol.RolPermisos.Add(new RolPermiso { RolId = rol.Id, PermisoId = permisoId });
                }

                await _context.SaveChangesAsync();

                Response<RolDTO> response = await GetOneAsync(rol.Id);
                response.Message = "Rol actualizado con éxito";

                return response;
            }
            catch (Exception ex)
            {
                return Response<RolDTO>.Failure(ex);
            }
        }

        public async Task<Response<object>> DeleteAsync(Guid id)
        {
            try
            {
                Rol? rol = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);

                if (rol is null)
                {
                    return Response<object>.Failure($"No existe rol con id {id}");
                }

                if (rol.Nombre == RolesBase.Administrador)
                {
                    return Response<object>.Failure("El rol Administrador no se puede eliminar");
                }

                if (await _context.Users.AnyAsync(u => u.RolId == id))
                {
                    return Response<object>.Failure("No se puede eliminar un rol que tiene usuarios asignados");
                }

                _context.Roles.Remove(rol);
                await _context.SaveChangesAsync();

                return Response<object>.Success("Rol eliminado con éxito");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        private async Task<List<Guid>> PermisosValidosAsync(List<Guid> permisoIds)
        {
            List<Guid> ids = permisoIds.Distinct().ToList();

            return await _context.Permisos.Where(p => ids.Contains(p.Id))
                                          .Select(p => p.Id)
                                          .ToListAsync();
        }

        private IQueryable<RolDTO> Proyectar(IQueryable<Rol> query)
        {
            return query.Select(r => new RolDTO
            {
                Id = r.Id,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                CantidadUsuarios = r.Usuarios.Count,
                Permisos = r.RolPermisos.Select(rp => new PermisoDTO
                {
                    Id = rp.Permiso.Id,
                    Codigo = rp.Permiso.Codigo,
                    Modulo = rp.Permiso.Modulo,
                    Descripcion = rp.Permiso.Descripcion,
                }).ToList(),
            });
        }
    }
}
