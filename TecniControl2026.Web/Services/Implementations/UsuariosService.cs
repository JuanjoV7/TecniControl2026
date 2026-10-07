using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.DTOs.Usuario;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    public class UsuariosService : CustomQueryableOperationsService, IUsuariosService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<Usuario> _userManager;
        private readonly IUsuarioActualService _usuarioActual;

        public UsuariosService(DataContext context, IMapper mapper, UserManager<Usuario> userManager, IUsuarioActualService usuarioActual)
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _usuarioActual = usuarioActual;
        }

        public async Task<Response<UsuarioDTO>> CreateAsync(CreateUsuarioDTO dto)
        {
            try
            {
                dto.Documento = dto.Documento.Trim();
                dto.Email = dto.Email.Trim();

                string? error = await ValidarAsync(dto.Documento, dto.Email, dto.RolId, null);

                if (error is not null)
                {
                    return Response<UsuarioDTO>.Failure(error);
                }

                Usuario usuario = new Usuario
                {
                    Id = Guid.NewGuid(),
                    UserName = dto.Email,
                    Email = dto.Email,
                    EmailConfirmed = true,
                    Documento = dto.Documento,
                    Nombres = dto.Nombres.Trim(),
                    Apellidos = dto.Apellidos.Trim(),
                    PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim(),
                    RolId = dto.RolId,
                    Activo = true,
                };

                IdentityResult result = await _userManager.CreateAsync(usuario, dto.Password);

                if (!result.Succeeded)
                {
                    return Response<UsuarioDTO>.Failure("No fue posible registrar el usuario", Errores(result));
                }

                Response<UsuarioDTO> response = await GetOneAsync(usuario.Id);
                response.Message = "Usuario registrado con éxito";

                return response;
            }
            catch (Exception ex)
            {
                return Response<UsuarioDTO>.Failure(ex);
            }
        }

        public async Task<Response<UsuarioDTO>> GetOneAsync(Guid id)
        {
            return await GetOneAsync<UsuarioDTO, Usuario>(id, _context.Users.Include(u => u.Rol));
        }

        public async Task<Response<PaginationResponse<UsuarioDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            IQueryable<Usuario> query = _context.Users.Include(u => u.Rol).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                string filter = request.Filter.Trim().ToLower();

                query = query.Where(u => u.Nombres.ToLower().Contains(filter)
                                      || u.Apellidos.ToLower().Contains(filter)
                                      || u.Documento.ToLower().Contains(filter)
                                      || (u.Email != null && u.Email.ToLower().Contains(filter))
                                      || u.Rol.Nombre.ToLower().Contains(filter));
            }

            query = query.OrderBy(u => u.Nombres).ThenBy(u => u.Apellidos);

            return await GetPagedListAsync<UsuarioDTO, Usuario>(request, query);
        }

        // Usuarios activos cuyo rol permite atender órdenes
        public async Task<Response<List<UsuarioDTO>>> GetTecnicosAsync()
        {
            try
            {
                List<Usuario> tecnicos = await _context.Users.Include(u => u.Rol)
                                                             .Where(u => u.Activo
                                                                      && u.Rol.RolPermisos.Any(rp => rp.Permiso.Codigo == PermisosCatalogo.Ordenes.Atender))
                                                             .OrderBy(u => u.Nombres)
                                                             .ThenBy(u => u.Apellidos)
                                                             .ToListAsync();

                return Response<List<UsuarioDTO>>.Success(_mapper.Map<List<UsuarioDTO>>(tecnicos));
            }
            catch (Exception ex)
            {
                return Response<List<UsuarioDTO>>.Failure(ex);
            }
        }

        public async Task<Response<UsuarioDTO>> UpdateAsync(UpdateUsuarioDTO dto)
        {
            try
            {
                Usuario? usuario = await _userManager.FindByIdAsync(dto.Id.ToString());

                if (usuario is null)
                {
                    return Response<UsuarioDTO>.Failure($"No existe usuario con id {dto.Id}");
                }

                dto.Documento = dto.Documento.Trim();
                dto.Email = dto.Email.Trim();

                string? error = await ValidarAsync(dto.Documento, dto.Email, dto.RolId, dto.Id);

                if (error is not null)
                {
                    return Response<UsuarioDTO>.Failure(error);
                }

                if (usuario.Id == _usuarioActual.UsuarioId && usuario.RolId != dto.RolId)
                {
                    return Response<UsuarioDTO>.Failure("No puede cambiar su propio rol");
                }

                usuario.Documento = dto.Documento;
                usuario.Nombres = dto.Nombres.Trim();
                usuario.Apellidos = dto.Apellidos.Trim();
                usuario.Email = dto.Email;
                usuario.UserName = dto.Email;
                usuario.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();
                usuario.RolId = dto.RolId;

                IdentityResult result = await _userManager.UpdateAsync(usuario);

                if (!result.Succeeded)
                {
                    return Response<UsuarioDTO>.Failure("No fue posible actualizar el usuario", Errores(result));
                }

                Response<UsuarioDTO> response = await GetOneAsync(usuario.Id);
                response.Message = "Usuario actualizado con éxito";

                return response;
            }
            catch (Exception ex)
            {
                return Response<UsuarioDTO>.Failure(ex);
            }
        }

        // Los usuarios no se eliminan, se desactivan para conservar el historial
        public async Task<Response<object>> ToggleAsync(ToggleUsuarioStatusDTO dto)
        {
            try
            {
                Usuario? usuario = await _userManager.FindByIdAsync(dto.UsuarioId.ToString());

                if (usuario is null)
                {
                    return Response<object>.Failure($"No existe usuario con id: {dto.UsuarioId}");
                }

                if (usuario.Id == _usuarioActual.UsuarioId && !dto.Activo)
                {
                    return Response<object>.Failure("No puede desactivar su propio usuario");
                }

                usuario.Activo = dto.Activo;

                IdentityResult result = await _userManager.UpdateAsync(usuario);

                if (!result.Succeeded)
                {
                    return Response<object>.Failure("No fue posible actualizar el usuario", Errores(result));
                }

                if (!dto.Activo)
                {
                    // Invalida las sesiones abiertas del usuario
                    await _userManager.UpdateSecurityStampAsync(usuario);
                }

                return Response<object>.Success(dto.Activo ? "Usuario activado con éxito" : "Usuario desactivado con éxito");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        private async Task<string?> ValidarAsync(string documento, string email, Guid rolId, Guid? usuarioId)
        {
            if (await _context.Users.AnyAsync(u => u.Documento == documento && u.Id != usuarioId))
            {
                return "Ya existe un usuario con ese documento";
            }

            string emailNormalizado = _userManager.NormalizeEmail(email);

            if (await _context.Users.AnyAsync(u => u.NormalizedEmail == emailNormalizado && u.Id != usuarioId))
            {
                return "Ya existe un usuario con ese correo electrónico";
            }

            if (!await _context.Roles.AnyAsync(r => r.Id == rolId))
            {
                return "El rol seleccionado no existe";
            }

            return null;
        }

        private static List<string> Errores(IdentityResult result)
        {
            return result.Errors.Select(e => e.Description).ToList();
        }
    }
}
