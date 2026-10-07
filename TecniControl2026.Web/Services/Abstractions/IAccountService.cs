using TecniControl2026.Web.Core;
using TecniControl2026.Web.DTOs.Account;

namespace TecniControl2026.Web.Services.Abstractions
{
    public interface IAccountService
    {
        public Task<Response<object>> LoginAsync(LoginDTO dto);
        public Task LogoutAsync();
        public Task<Response<object>> ChangePasswordAsync(ChangePasswordDTO dto);
        public Task<Response<PerfilDTO>> GetPerfilAsync();
        public Task<Response<PerfilDTO>> UpdatePerfilAsync(PerfilDTO dto);

        // urlRestablecer: URL de la página de restablecimiento; se le agregan email y token
        public Task<Response<object>> ForgotPasswordAsync(ForgotPasswordDTO dto, string urlRestablecer);
        public Task<Response<object>> ResetPasswordAsync(ResetPasswordDTO dto);
    }
}
