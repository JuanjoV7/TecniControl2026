using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.DTOs.User;

namespace TecniControl2026.Web.Services.Abstractions
{
    public interface IUsersService
    {
        public Task<Response<UserDTO>> CreateAsync(CreateUserDTO dto);
        public Task<Response<UserDTO>> GetOneAsync(Guid id);
        public Task<Response<PaginationResponse<UserDTO>>> GetPaginationAsync(PaginationRequest request);
        public Task<Response<List<UserDTO>>> GetTechniciansAsync();
        public Task<Response<UserDTO>> UpdateAsync(UpdateUserDTO dto);
        public Task<Response<object>> ToggleAsync(ToggleUserStatusDTO dto);
    }
}
