using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.DTOs.Role;
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

        public async Task<Response<RoleDTO>> CreateAsync(CreateRoleDTO dto)
        {
            try
            {
                string name = dto.Name.Trim();

                if (await _context.Roles.AnyAsync(r => r.Name == name))
                {
                    return Response<RoleDTO>.Failure("Ya existe un rol con ese nombre");
                }

                List<Guid> permissionIds = await ValidPermissionsAsync(dto.PermissionIds);

                Role role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                };

                foreach (Guid permissionId in permissionIds)
                {
                    role.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permissionId });
                }

                await _context.Roles.AddAsync(role);
                await _context.SaveChangesAsync();

                Response<RoleDTO> response = await GetOneAsync(role.Id);
                response.Message = "Rol creado con éxito";

                return response;
            }
            catch (Exception ex)
            {
                return Response<RoleDTO>.Failure(ex);
            }
        }

        public async Task<Response<RoleDTO>> GetOneAsync(Guid id)
        {
            try
            {
                RoleDTO? role = await ProjectToDto(_context.Roles.Where(r => r.Id == id)).FirstOrDefaultAsync();

                if (role is null)
                {
                    return Response<RoleDTO>.Failure($"No existe rol con id {id}");
                }

                return Response<RoleDTO>.Success(role, "Rol obtenido con éxito");
            }
            catch (Exception ex)
            {
                return Response<RoleDTO>.Failure(ex);
            }
        }

        public async Task<Response<PaginationResponse<RoleDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            try
            {
                IQueryable<Role> query = _context.Roles.AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.Filter))
                {
                    string filter = request.Filter.Trim().ToLower();

                    query = query.Where(r => r.Name.ToLower().Contains(filter)
                                          || (r.Description != null && r.Description.ToLower().Contains(filter)));
                }

                PagedList<RoleDTO> list = await PagedList<RoleDTO>.ToPagedListAsync(ProjectToDto(query.OrderBy(r => r.Name)), request);

                PaginationResponse<RoleDTO> dto = new PaginationResponse<RoleDTO>
                {
                    List = list,
                    CurrentPage = list.CurrentPage,
                    TotalPages = list.TotalPages,
                    RecordsPerPage = list.RecordsPerPage,
                    TotalCount = list.TotalCount,
                    Filter = request.Filter,
                };

                return Response<PaginationResponse<RoleDTO>>.Success(dto, "Registros obtenidos con éxito");
            }
            catch (Exception ex)
            {
                return Response<PaginationResponse<RoleDTO>>.Failure(ex);
            }
        }

        public async Task<Response<List<RoleDTO>>> GetAllAsync()
        {
            try
            {
                List<RoleDTO> roles = await ProjectToDto(_context.Roles.OrderBy(r => r.Name)).ToListAsync();

                return Response<List<RoleDTO>>.Success(roles);
            }
            catch (Exception ex)
            {
                return Response<List<RoleDTO>>.Failure(ex);
            }
        }

        public async Task<Response<List<PermissionDTO>>> GetPermissionsAsync()
        {
            try
            {
                List<Permission> permissions = await _context.Permissions.OrderBy(p => p.Module)
                                                                .ThenBy(p => p.Code)
                                                                .ToListAsync();

                return Response<List<PermissionDTO>>.Success(_mapper.Map<List<PermissionDTO>>(permissions));
            }
            catch (Exception ex)
            {
                return Response<List<PermissionDTO>>.Failure(ex);
            }
        }

        public async Task<Response<RoleDTO>> UpdateAsync(UpdateRoleDTO dto)
        {
            try
            {
                Role? role = await _context.Roles.Include(r => r.RolePermissions)
                                               .FirstOrDefaultAsync(r => r.Id == dto.Id);

                if (role is null)
                {
                    return Response<RoleDTO>.Failure($"No existe rol con id {dto.Id}");
                }

                // The administrator always keeps every permission (see SeedDb)
                if (role.Name == BaseRoles.Administrator)
                {
                    return Response<RoleDTO>.Failure("El rol Administrador no se puede modificar");
                }

                string name = dto.Name.Trim();

                if (await _context.Roles.AnyAsync(r => r.Name == name && r.Id != dto.Id))
                {
                    return Response<RoleDTO>.Failure("Ya existe otro rol con ese nombre");
                }

                role.Name = name;
                role.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

                List<Guid> permissionIds = await ValidPermissionsAsync(dto.PermissionIds);

                List<RolePermission> removed = role.RolePermissions.Where(rp => !permissionIds.Contains(rp.PermissionId)).ToList();
                _context.RolePermissions.RemoveRange(removed);

                IEnumerable<Guid> newPermissionIds = permissionIds.Where(id => !role.RolePermissions.Any(rp => rp.PermissionId == id));

                foreach (Guid permissionId in newPermissionIds)
                {
                    role.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permissionId });
                }

                await _context.SaveChangesAsync();

                Response<RoleDTO> response = await GetOneAsync(role.Id);
                response.Message = "Rol actualizado con éxito";

                return response;
            }
            catch (Exception ex)
            {
                return Response<RoleDTO>.Failure(ex);
            }
        }

        public async Task<Response<object>> DeleteAsync(Guid id)
        {
            try
            {
                Role? role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);

                if (role is null)
                {
                    return Response<object>.Failure($"No existe rol con id {id}");
                }

                if (role.Name == BaseRoles.Administrator)
                {
                    return Response<object>.Failure("El rol Administrador no se puede eliminar");
                }

                if (await _context.Users.AnyAsync(u => u.RoleId == id))
                {
                    return Response<object>.Failure("No se puede eliminar un rol que tiene usuarios asignados");
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                return Response<object>.Success("Rol eliminado con éxito");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        private async Task<List<Guid>> ValidPermissionsAsync(List<Guid> permissionIds)
        {
            List<Guid> ids = permissionIds.Distinct().ToList();

            return await _context.Permissions.Where(p => ids.Contains(p.Id))
                                          .Select(p => p.Id)
                                          .ToListAsync();
        }

        private IQueryable<RoleDTO> ProjectToDto(IQueryable<Role> query)
        {
            return query.Select(r => new RoleDTO
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                UserCount = r.Users.Count,
                Permissions = r.RolePermissions.Select(rp => new PermissionDTO
                {
                    Id = rp.Permission.Id,
                    Code = rp.Permission.Code,
                    Module = rp.Permission.Module,
                    Description = rp.Permission.Description,
                }).ToList(),
            });
        }
    }
}
