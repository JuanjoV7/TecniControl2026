using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.DTOs.FollowUp;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    public class FollowUpsService : IFollowUpsService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public FollowUpsService(DataContext context, IMapper mapper, ICurrentUserService currentUser)
        {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<Response<FollowUpDTO>> CreateAsync(CreateFollowUpDTO dto)
        {
            try
            {
                ServiceOrder? order = await _context.ServiceOrders.AsNoTracking()
                                                                     .FirstOrDefaultAsync(o => o.Id == dto.ServiceOrderId);

                if (order is null || !await CanViewAsync(order))
                {
                    return Response<FollowUpDTO>.Failure($"No existe orden con id {dto.ServiceOrderId}");
                }

                FollowUp followUp = new FollowUp
                {
                    Id = Guid.NewGuid(),
                    ServiceOrderId = order.Id,
                    UserId = _currentUser.UserId!.Value,
                    Date = DateTime.Now,
                    Observation = dto.Observation.Trim(),
                };

                await _context.FollowUps.AddAsync(followUp);
                await _context.SaveChangesAsync();

                await _context.Entry(followUp).Reference(s => s.User).LoadAsync();

                return Response<FollowUpDTO>.Success(_mapper.Map<FollowUpDTO>(followUp), "Observación registrada con éxito");
            }
            catch (Exception ex)
            {
                return Response<FollowUpDTO>.Failure(ex);
            }
        }

        public async Task<Response<List<FollowUpDTO>>> GetByOrderAsync(Guid serviceOrderId)
        {
            try
            {
                ServiceOrder? order = await _context.ServiceOrders.AsNoTracking()
                                                                     .FirstOrDefaultAsync(o => o.Id == serviceOrderId);

                if (order is null || !await CanViewAsync(order))
                {
                    return Response<List<FollowUpDTO>>.Failure($"No existe orden con id {serviceOrderId}");
                }

                List<FollowUp> followUps = await _context.FollowUps.Include(s => s.User)
                                                                            .Where(s => s.ServiceOrderId == serviceOrderId)
                                                                            .OrderByDescending(s => s.Date)
                                                                            .ToListAsync();

                return Response<List<FollowUpDTO>>.Success(_mapper.Map<List<FollowUpDTO>>(followUps));
            }
            catch (Exception ex)
            {
                return Response<List<FollowUpDTO>>.Failure(ex);
            }
        }

        private async Task<bool> CanViewAsync(ServiceOrder order)
        {
            if (await _currentUser.HasPermissionAsync(PermissionsCatalog.Orders.ShowAll))
            {
                return true;
            }

            return order.TechnicianId is not null && order.TechnicianId == _currentUser.UserId;
        }
    }
}
