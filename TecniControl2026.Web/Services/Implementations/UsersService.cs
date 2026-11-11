using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.DTOs.User;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    public class UsersService : CustomQueryableOperationsService, IUsersService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly ICurrentUserService _currentUser;

        public UsersService(DataContext context, IMapper mapper, UserManager<User> userManager, ICurrentUserService currentUser)
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _currentUser = currentUser;
        }

        public async Task<Response<UserDTO>> CreateAsync(CreateUserDTO dto)
        {
            try
            {
                dto.Document = dto.Document.Trim();
                dto.Email = dto.Email.Trim();

                string? error = await ValidateAsync(dto.Document, dto.Email, dto.RoleId, null);

                if (error is not null)
                {
                    return Response<UserDTO>.Failure(error);
                }

                User user = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = dto.Email,
                    Email = dto.Email,
                    EmailConfirmed = true,
                    Document = dto.Document,
                    FirstName = dto.FirstName.Trim(),
                    LastName = dto.LastName.Trim(),
                    PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim(),
                    RoleId = dto.RoleId,
                    IsActive = true,
                };

                IdentityResult result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    return Response<UserDTO>.Failure("No fue posible registrar el usuario", Errors(result));
                }

                Response<UserDTO> response = await GetOneAsync(user.Id);
                response.Message = "Usuario registrado con éxito";

                return response;
            }
            catch (Exception ex)
            {
                return Response<UserDTO>.Failure(ex);
            }
        }

        public async Task<Response<UserDTO>> GetOneAsync(Guid id)
        {
            return await GetOneAsync<UserDTO, User>(id, _context.Users.Include(u => u.Role));
        }

        public async Task<Response<PaginationResponse<UserDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            IQueryable<User> query = _context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                string filter = request.Filter.Trim().ToLower();

                query = query.Where(u => u.FirstName.ToLower().Contains(filter)
                                      || u.LastName.ToLower().Contains(filter)
                                      || u.Document.ToLower().Contains(filter)
                                      || (u.Email != null && u.Email.ToLower().Contains(filter))
                                      || u.Role.Name.ToLower().Contains(filter));
            }

            query = query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName);

            return await GetPagedListAsync<UserDTO, User>(request, query);
        }

        // Active users whose role allows attending orders
        public async Task<Response<List<UserDTO>>> GetTechniciansAsync()
        {
            try
            {
                List<User> technicians = await _context.Users.Include(u => u.Role)
                                                             .Where(u => u.IsActive
                                                                      && u.Role.RolePermissions.Any(rp => rp.Permission.Code == PermissionsCatalog.Orders.Attend))
                                                             .OrderBy(u => u.FirstName)
                                                             .ThenBy(u => u.LastName)
                                                             .ToListAsync();

                return Response<List<UserDTO>>.Success(_mapper.Map<List<UserDTO>>(technicians));
            }
            catch (Exception ex)
            {
                return Response<List<UserDTO>>.Failure(ex);
            }
        }

        public async Task<Response<UserDTO>> UpdateAsync(UpdateUserDTO dto)
        {
            try
            {
                User? user = await _userManager.FindByIdAsync(dto.Id.ToString());

                if (user is null)
                {
                    return Response<UserDTO>.Failure($"No existe usuario con id {dto.Id}");
                }

                dto.Document = dto.Document.Trim();
                dto.Email = dto.Email.Trim();

                string? error = await ValidateAsync(dto.Document, dto.Email, dto.RoleId, dto.Id);

                if (error is not null)
                {
                    return Response<UserDTO>.Failure(error);
                }

                if (user.Id == _currentUser.UserId && user.RoleId != dto.RoleId)
                {
                    return Response<UserDTO>.Failure("No puede cambiar su propio rol");
                }

                user.Document = dto.Document;
                user.FirstName = dto.FirstName.Trim();
                user.LastName = dto.LastName.Trim();
                user.Email = dto.Email;
                user.UserName = dto.Email;
                user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();
                user.RoleId = dto.RoleId;

                IdentityResult result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return Response<UserDTO>.Failure("No fue posible actualizar el usuario", Errors(result));
                }

                Response<UserDTO> response = await GetOneAsync(user.Id);
                response.Message = "Usuario actualizado con éxito";

                return response;
            }
            catch (Exception ex)
            {
                return Response<UserDTO>.Failure(ex);
            }
        }

        // Users are not deleted, they are deactivated to keep the history
        public async Task<Response<object>> ToggleAsync(ToggleUserStatusDTO dto)
        {
            try
            {
                User? user = await _userManager.FindByIdAsync(dto.UserId.ToString());

                if (user is null)
                {
                    return Response<object>.Failure($"No existe usuario con id: {dto.UserId}");
                }

                if (user.Id == _currentUser.UserId && !dto.IsActive)
                {
                    return Response<object>.Failure("No puede desactivar su propio usuario");
                }

                user.IsActive = dto.IsActive;

                IdentityResult result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return Response<object>.Failure("No fue posible actualizar el usuario", Errors(result));
                }

                if (!dto.IsActive)
                {
                    // Invalidates the user's open sessions
                    await _userManager.UpdateSecurityStampAsync(user);
                }

                return Response<object>.Success(dto.IsActive ? "Usuario activado con éxito" : "Usuario desactivado con éxito");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        private async Task<string?> ValidateAsync(string document, string email, Guid roleId, Guid? userId)
        {
            if (await _context.Users.AnyAsync(u => u.Document == document && u.Id != userId))
            {
                return "Ya existe un usuario con ese documento";
            }

            string normalizedEmail = _userManager.NormalizeEmail(email);

            if (await _context.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail && u.Id != userId))
            {
                return "Ya existe un usuario con ese correo electrónico";
            }

            if (!await _context.Roles.AnyAsync(r => r.Id == roleId))
            {
                return "El rol seleccionado no existe";
            }

            return null;
        }

        private static List<string> Errors(IdentityResult result)
        {
            return result.Errors.Select(e => e.Description).ToList();
        }
    }
}
