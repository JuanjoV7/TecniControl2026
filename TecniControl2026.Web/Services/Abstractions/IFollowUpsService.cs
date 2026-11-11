using TecniControl2026.Web.Core;
using TecniControl2026.Web.DTOs.FollowUp;

namespace TecniControl2026.Web.Services.Abstractions
{
    public interface IFollowUpsService
    {
        public Task<Response<FollowUpDTO>> CreateAsync(CreateFollowUpDTO dto);
        public Task<Response<List<FollowUpDTO>>> GetByOrderAsync(Guid serviceOrderId);
    }
}
