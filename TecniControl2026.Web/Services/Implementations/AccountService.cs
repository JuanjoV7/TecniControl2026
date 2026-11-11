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
        private const string RecoveryMessage = "Si el correo está registrado, recibirá un enlace para restablecer la contraseña";

        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ICurrentUserService _currentUser;
        private readonly IEmailService _emailService;

        public AccountService(DataContext context,
                              UserManager<User> userManager,
                              SignInManager<User> signInManager,
                              ICurrentUserService currentUser,
                              IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _currentUser = currentUser;
            _emailService = emailService;
        }

        public async Task<Response<SignInResult>> LoginAsync(LoginDTO dto)
        {
            SignInResult result = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, false, false);

            return new Response<SignInResult>
            {
                Result = result,
                IsSuccess = result.Succeeded
            };
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<Response<object>> ChangePasswordAsync(ChangePasswordDTO dto)
        {
            try
            {
                User? user = await GetCurrentUserAsync();

                if (user is null)
                {
                    return Response<object>.Failure("No hay un usuario autenticado");
                }

                IdentityResult result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

                if (!result.Succeeded)
                {
                    return Response<object>.Failure("No fue posible cambiar la contraseña", result.Errors.Select(e => e.Description).ToList());
                }

                // Changing the password renews the security stamp: the cookie is reissued so the current session stays open
                await _signInManager.RefreshSignInAsync(user);

                return Response<object>.Success("Contraseña actualizada con éxito");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        public async Task<Response<ProfileDTO>> GetProfileAsync()
        {
            try
            {
                Guid? userId = _currentUser.UserId;

                ProfileDTO? profile = await _context.Users.Where(u => u.Id == userId)
                                                        .Select(u => new ProfileDTO
                                                        {
                                                            Email = u.Email,
                                                            Document = u.Document,
                                                            RoleName = u.Role.Name,
                                                            FirstName = u.FirstName,
                                                            LastName = u.LastName,
                                                            PhoneNumber = u.PhoneNumber,
                                                        })
                                                        .FirstOrDefaultAsync();

                if (profile is null)
                {
                    return Response<ProfileDTO>.Failure("No hay un usuario autenticado");
                }

                return Response<ProfileDTO>.Success(profile);
            }
            catch (Exception ex)
            {
                return Response<ProfileDTO>.Failure(ex);
            }
        }

        // The user can only change personal data; email, document and role are managed by the administrator
        public async Task<Response<ProfileDTO>> UpdateProfileAsync(ProfileDTO dto)
        {
            try
            {
                User? user = await GetCurrentUserAsync();

                if (user is null)
                {
                    return Response<ProfileDTO>.Failure("No hay un usuario autenticado");
                }

                user.FirstName = dto.FirstName.Trim();
                user.LastName = dto.LastName.Trim();
                user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();

                IdentityResult result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return Response<ProfileDTO>.Failure("No fue posible actualizar el perfil", result.Errors.Select(e => e.Description).ToList());
                }

                // Updates the name shown in the header
                await _signInManager.RefreshSignInAsync(user);

                Response<ProfileDTO> response = await GetProfileAsync();
                response.Message = "Perfil actualizado con éxito";

                return response;
            }
            catch (Exception ex)
            {
                return Response<ProfileDTO>.Failure(ex);
            }
        }

        public async Task<Response<object>> ForgotPasswordAsync(ForgotPasswordDTO dto, string resetUrl)
        {
            try
            {
                User? user = await _userManager.FindByEmailAsync(dto.Email.Trim());

                // The same response is returned whether or not the email exists, so existing accounts are not revealed
                if (user is null || !user.IsActive)
                {
                    return Response<object>.Success(RecoveryMessage);
                }

                string token = await _userManager.GeneratePasswordResetTokenAsync(user);
                string encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                string link = QueryHelpers.AddQueryString(resetUrl, new Dictionary<string, string?>
                {
                    ["email"] = user.Email,
                    ["token"] = encodedToken,
                });

                string body = $"<p>Hola {HtmlEncoder.Default.Encode(user.FirstName)},</p>"
                              + $"<p>Para restablecer su contraseña de TecniControl haga clic <a href=\"{HtmlEncoder.Default.Encode(link)}\">aquí</a>.</p>"
                              + "<p>Si usted no lo solicitó, ignore este mensaje.</p>";

                await _emailService.SendAsync(user.Email!, "TecniControl - Recuperación de contraseña", body);

                return Response<object>.Success(RecoveryMessage);
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
                User? user = await _userManager.FindByEmailAsync(dto.Email.Trim());

                if (user is null)
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

                IdentityResult result = await _userManager.ResetPasswordAsync(user, token, dto.Password);

                if (!result.Succeeded)
                {
                    bool invalidToken = result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.InvalidToken));

                    return Response<object>.Failure(invalidToken ? "El enlace de recuperación no es válido o ya expiró" : "No fue posible restablecer la contraseña",
                                                    result.Errors.Select(e => e.Description).ToList());
                }

                return Response<object>.Success("Contraseña restablecida con éxito. Ya puede iniciar sesión");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            Guid? userId = _currentUser.UserId;

            return userId is null ? null : await _userManager.FindByIdAsync(userId.Value.ToString());
        }
    }
}