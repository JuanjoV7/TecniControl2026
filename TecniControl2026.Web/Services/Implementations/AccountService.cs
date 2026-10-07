using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.DTOs.Account;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private const string MensajeRecuperacion = "Si el correo está registrado, recibirá un enlace para restablecer la contraseña";

        private readonly DataContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IUsuarioActualService _usuarioActual;
        private readonly ICorreoService _correoService;

        public AccountService(DataContext context,
                              UserManager<Usuario> userManager,
                              SignInManager<Usuario> signInManager,
                              IUsuarioActualService usuarioActual,
                              ICorreoService correoService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _usuarioActual = usuarioActual;
            _correoService = correoService;
        }

        public async Task<Response<object>> LoginAsync(LoginDTO dto)
        {
            try
            {
                Usuario? usuario = await _userManager.FindByEmailAsync(dto.Email.Trim());

                if (usuario is null)
                {
                    return Response<object>.Failure("Correo o contraseña incorrectos");
                }

                if (!usuario.Activo)
                {
                    return Response<object>.Failure("El usuario está desactivado. Comuníquese con el administrador");
                }

                SignInResult result = await _signInManager.PasswordSignInAsync(usuario, dto.Password, dto.RememberMe, lockoutOnFailure: true);

                if (result.IsLockedOut)
                {
                    return Response<object>.Failure("Usuario bloqueado temporalmente por varios intentos fallidos. Intente más tarde");
                }

                if (!result.Succeeded)
                {
                    return Response<object>.Failure("Correo o contraseña incorrectos");
                }

                return Response<object>.Success($"Bienvenido(a), {usuario.Nombres}");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<Response<object>> ChangePasswordAsync(ChangePasswordDTO dto)
        {
            try
            {
                Usuario? usuario = await GetUsuarioActualAsync();

                if (usuario is null)
                {
                    return Response<object>.Failure("No hay un usuario autenticado");
                }

                IdentityResult result = await _userManager.ChangePasswordAsync(usuario, dto.CurrentPassword, dto.NewPassword);

                if (!result.Succeeded)
                {
                    return Response<object>.Failure("No fue posible cambiar la contraseña", result.Errors.Select(e => e.Description).ToList());
                }

                // Cambiar la contraseña renueva el security stamp: se reemite la cookie para no cerrar la sesión actual
                await _signInManager.RefreshSignInAsync(usuario);

                return Response<object>.Success("Contraseña actualizada con éxito");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        public async Task<Response<PerfilDTO>> GetPerfilAsync()
        {
            try
            {
                Guid? usuarioId = _usuarioActual.UsuarioId;

                PerfilDTO? perfil = await _context.Users.Where(u => u.Id == usuarioId)
                                                        .Select(u => new PerfilDTO
                                                        {
                                                            Email = u.Email,
                                                            Documento = u.Documento,
                                                            RolNombre = u.Rol.Nombre,
                                                            Nombres = u.Nombres,
                                                            Apellidos = u.Apellidos,
                                                            PhoneNumber = u.PhoneNumber,
                                                        })
                                                        .FirstOrDefaultAsync();

                if (perfil is null)
                {
                    return Response<PerfilDTO>.Failure("No hay un usuario autenticado");
                }

                return Response<PerfilDTO>.Success(perfil);
            }
            catch (Exception ex)
            {
                return Response<PerfilDTO>.Failure(ex);
            }
        }

        // El usuario solo puede cambiar sus datos personales; correo, documento y rol los gestiona el administrador
        public async Task<Response<PerfilDTO>> UpdatePerfilAsync(PerfilDTO dto)
        {
            try
            {
                Usuario? usuario = await GetUsuarioActualAsync();

                if (usuario is null)
                {
                    return Response<PerfilDTO>.Failure("No hay un usuario autenticado");
                }

                usuario.Nombres = dto.Nombres.Trim();
                usuario.Apellidos = dto.Apellidos.Trim();
                usuario.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();

                IdentityResult result = await _userManager.UpdateAsync(usuario);

                if (!result.Succeeded)
                {
                    return Response<PerfilDTO>.Failure("No fue posible actualizar el perfil", result.Errors.Select(e => e.Description).ToList());
                }

                // Actualiza el nombre que se muestra en el encabezado
                await _signInManager.RefreshSignInAsync(usuario);

                Response<PerfilDTO> response = await GetPerfilAsync();
                response.Message = "Perfil actualizado con éxito";

                return response;
            }
            catch (Exception ex)
            {
                return Response<PerfilDTO>.Failure(ex);
            }
        }

        public async Task<Response<object>> ForgotPasswordAsync(ForgotPasswordDTO dto, string urlRestablecer)
        {
            try
            {
                Usuario? usuario = await _userManager.FindByEmailAsync(dto.Email.Trim());

                // Se responde lo mismo exista o no el correo, para no revelar qué cuentas existen
                if (usuario is null || !usuario.Activo)
                {
                    return Response<object>.Success(MensajeRecuperacion);
                }

                string token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                string tokenCodificado = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                string enlace = QueryHelpers.AddQueryString(urlRestablecer, new Dictionary<string, string?>
                {
                    ["email"] = usuario.Email,
                    ["token"] = tokenCodificado,
                });

                string cuerpo = $"<p>Hola {HtmlEncoder.Default.Encode(usuario.Nombres)},</p>"
                              + $"<p>Para restablecer su contraseña de TecniControl haga clic <a href=\"{HtmlEncoder.Default.Encode(enlace)}\">aquí</a>.</p>"
                              + "<p>Si usted no lo solicitó, ignore este mensaje.</p>";

                await _correoService.EnviarAsync(usuario.Email!, "TecniControl - Recuperación de contraseña", cuerpo);

                return Response<object>.Success(MensajeRecuperacion);
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        public async Task<Response<object>> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            try
            {
                Usuario? usuario = await _userManager.FindByEmailAsync(dto.Email.Trim());

                if (usuario is null)
                {
                    return Response<object>.Failure("El enlace de recuperación no es válido o ya expiró");
                }

                string token;

                try
                {
                    token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
                }
                catch (FormatException)
                {
                    return Response<object>.Failure("El enlace de recuperación no es válido o ya expiró");
                }

                IdentityResult result = await _userManager.ResetPasswordAsync(usuario, token, dto.Password);

                if (!result.Succeeded)
                {
                    bool tokenInvalido = result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.InvalidToken));

                    return Response<object>.Failure(tokenInvalido ? "El enlace de recuperación no es válido o ya expiró" : "No fue posible restablecer la contraseña",
                                                    result.Errors.Select(e => e.Description).ToList());
                }

                return Response<object>.Success("Contraseña restablecida con éxito. Ya puede iniciar sesión");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        private async Task<Usuario?> GetUsuarioActualAsync()
        {
            Guid? usuarioId = _usuarioActual.UsuarioId;

            return usuarioId is null ? null : await _userManager.FindByIdAsync(usuarioId.Value.ToString());
        }
    }
}
