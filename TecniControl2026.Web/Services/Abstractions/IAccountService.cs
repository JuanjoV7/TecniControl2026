using TecniControl2026.Web.Core;
using TecniControl2026.Web.DTOs.Account;

namespace TecniControl2026.Web.Services.Abstractions
{
    public interface IAccountService
    {
        public Task<Response<object>> LoginAsync(LoginDTO dto);
        public Task LogoutAsync();
        public Task<Response<object>> ChangePasswordAsync(ChangePasswordDTO dto);
        public Task<Response<ProfileDTO>> GetProfileAsync();
        public Task<Response<ProfileDTO>> UpdateProfileAsync(ProfileDTO dto);

        // resetUrl: URL of the reset page; email and token are appended to it
        public Task<Response<object>> ForgotPasswordAsync(ForgotPasswordDTO dto, string resetUrl);
        public Task<Response<object>> ResetPasswordAsync(ResetPasswordDTO dto);
    }
}
