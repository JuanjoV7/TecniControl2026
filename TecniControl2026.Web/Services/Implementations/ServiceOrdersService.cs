using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Authorization;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.Data.Enums;
using TecniControl2026.Web.DTOs.ServiceOrder;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    public class ServiceOrdersService : CustomQueryableOperationsService, IServiceOrdersService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;
        private readonly IPermissionsService _permissionsService;

        public ServiceOrdersService(DataContext context, IMapper mapper, ICurrentUserService currentUser, IPermissionsService permissionsService)
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
            _permissionsService = permissionsService;
        }

        public async Task<Response<ServiceOrderDTO>> CreateAsync(CreateServiceOrderDTO dto)
        {
            try
            {
                Guid? userId = _currentUser.UserId;

                if (userId is null)
                {
                    return Response<ServiceOrderDTO>.Failure("No hay un usuario autenticado");
                }

                Device? device = await _context.Devices.Include(e => e.Customer)
                                                       .FirstOrDefaultAsync(e => e.Id == dto.DeviceId);

                if (device is null)
                {
                    return Response<ServiceOrderDTO>.Failure("El equipo seleccionado no existe");
                }

                if (!device.Customer.IsActive)
                {
                    return Response<ServiceOrderDTO>.Failure("El cliente del equipo está inactivo");
                }

                bool hasOpenOrder = await _context.ServiceOrders.AnyAsync(o => o.DeviceId == dto.DeviceId
                                                                                   && o.Status != OrderStatus.Delivered
                                                                                   && o.Status != OrderStatus.Cancelled);

                if (hasOpenOrder)
                {
                    return Response<ServiceOrderDTO>.Failure("El equipo ya tiene una orden de servicio abierta");
                }

                User? technician = null;

                if (dto.TechnicianId is not null)
                {
                    Response<User> technicianResponse = await ValidateTechnicianAsync(dto.TechnicianId.Value);

                    if (!technicianResponse.IsSuccess)
                    {
                        return Response<ServiceOrderDTO>.Failure(technicianResponse.Message!);
                    }

                    technician = technicianResponse.Result;
                }

                DateTime now = DateTime.Now;

                ServiceOrder order = new ServiceOrder
                {
                    Id = Guid.NewGuid(),
                    DeviceId = device.Id,
                    ReceptionDate = now,
                    ReceivedById = userId.Value,
                    ReportedIssue = dto.ReportedIssue.Trim(),
                    Accessories = string.IsNullOrWhiteSpace(dto.Accessories) ? null : dto.Accessories.Trim(),
                    TechnicianId = technician?.Id,
                    Status = OrderStatus.Received,
                };

                string observation = "Equipo recibido en el taller.";

                if (technician is not null)
                {
                    observation += $" Técnico asignado: {technician.FullName}.";
                }

                order.FollowUps.Add(NewFollowUp(userId.Value, now, observation, null, OrderStatus.Received));

                await _context.ServiceOrders.AddAsync(order);
                await _context.SaveChangesAsync();

                ServiceOrder created = await QueryWithDetail().FirstAsync(o => o.Id == order.Id);

                return Response<ServiceOrderDTO>.Success(_mapper.Map<ServiceOrderDTO>(created), $"Orden N° {created.Number} registrada con éxito");
            }
            catch (Exception ex)
            {
                return Response<ServiceOrderDTO>.Failure(ex);
            }
        }

        public async Task<Response<ServiceOrderDetailDTO>> GetOneAsync(Guid id)
        {
            try
            {
                ServiceOrder? order = await QueryWithDetail().Include(o => o.FollowUps)
                                                              .ThenInclude(s => s.User)
                                                              .AsSplitQuery()
                                                              .FirstOrDefaultAsync(o => o.Id == id);

                if (order is null || !await CanViewAsync(order))
                {
                    return Response<ServiceOrderDetailDTO>.Failure($"No existe orden con id {id}");
                }

                ServiceOrderDetailDTO dto = _mapper.Map<ServiceOrderDetailDTO>(order);
                dto.FollowUps = dto.FollowUps.OrderByDescending(s => s.Date).ToList();

                return Response<ServiceOrderDetailDTO>.Success(dto, "Orden obtenida con éxito");
            }
            catch (Exception ex)
            {
                return Response<ServiceOrderDetailDTO>.Failure(ex);
            }
        }

        public async Task<Response<PaginationResponse<ServiceOrderDTO>>> GetPaginationAsync(OrdersPaginationRequest request)
        {
            Response<IQueryable<ServiceOrder>> visible = await VisibleQuery();

            if (!visible.IsSuccess)
            {
                return Response<PaginationResponse<ServiceOrderDTO>>.Failure(visible.Message!);
            }

            IQueryable<ServiceOrder> query = visible.Result!;

            if (request.Status is not null)
            {
                query = query.Where(o => o.Status == request.Status);
            }

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                string filter = request.Filter.Trim().ToLower();

                if (int.TryParse(filter, out int number))
                {
                    query = query.Where(o => o.Number == number);
                }
                else
                {
                    query = query.Where(o => o.Device.Customer.Name.ToLower().Contains(filter)
                                          || o.Device.Customer.Document.ToLower().Contains(filter)
                                          || o.Device.Brand.ToLower().Contains(filter)
                                          || o.Device.DeviceModel.ToLower().Contains(filter)
                                          || (o.Device.SerialNumber != null && o.Device.SerialNumber.ToLower().Contains(filter)));
                }
            }

            query = query.OrderByDescending(o => o.ReceptionDate);

            return await GetPagedListAsync<ServiceOrderDTO, ServiceOrder>(request, query);
        }

        public async Task<Response<List<ServiceOrderDTO>>> GetDeviceHistoryAsync(Guid deviceId)
        {
            try
            {
                Response<IQueryable<ServiceOrder>> visible = await VisibleQuery();

                if (!visible.IsSuccess)
                {
                    return Response<List<ServiceOrderDTO>>.Failure(visible.Message!);
                }

                List<ServiceOrder> orders = await visible.Result!.Where(o => o.DeviceId == deviceId)
                                                                    .OrderByDescending(o => o.ReceptionDate)
                                                                    .ToListAsync();

                return Response<List<ServiceOrderDTO>>.Success(_mapper.Map<List<ServiceOrderDTO>>(orders));
            }
            catch (Exception ex)
            {
                return Response<List<ServiceOrderDTO>>.Failure(ex);
            }
        }

        public async Task<Response<object>> AssignTechnicianAsync(AssignTechnicianDTO dto)
        {
            try
            {
                ServiceOrder? order = await _context.ServiceOrders.Include(o => o.Technician)
                                                                     .FirstOrDefaultAsync(o => o.Id == dto.ServiceOrderId);

                if (order is null)
                {
                    return Response<object>.Failure($"No existe orden con id {dto.ServiceOrderId}");
                }

                if (OrderStatusRules.IsFinal(order.Status))
                {
                    return Response<object>.Failure("No se puede asignar técnico a una orden entregada o cancelada");
                }

                if (order.TechnicianId == dto.TechnicianId)
                {
                    return Response<object>.Failure("El técnico ya está asignado a esta orden");
                }

                Response<User> technicianResponse = await ValidateTechnicianAsync(dto.TechnicianId);

                if (!technicianResponse.IsSuccess)
                {
                    return Response<object>.Failure(technicianResponse.Message!);
                }

                User technician = technicianResponse.Result!;

                string observation = order.Technician is null
                    ? $"Técnico asignado: {technician.FullName}."
                    : $"Técnico cambiado de {order.Technician.FullName} a {technician.FullName}.";

                order.TechnicianId = technician.Id;
                order.FollowUps.Add(NewFollowUp(_currentUser.UserId!.Value, DateTime.Now, observation, null, null));

                await _context.SaveChangesAsync();

                return Response<object>.Success("Técnico asignado con éxito");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        public async Task<Response<object>> RegisterAttentionAsync(RegisterAttentionDTO dto)
        {
            try
            {
                ServiceOrder? order = await _context.ServiceOrders.FirstOrDefaultAsync(o => o.Id == dto.ServiceOrderId);

                if (order is null)
                {
                    return Response<object>.Failure($"No existe orden con id {dto.ServiceOrderId}");
                }

                if (OrderStatusRules.IsFinal(order.Status))
                {
                    return Response<object>.Failure("La orden ya fue entregada o cancelada");
                }

                if (order.TechnicianId is null)
                {
                    return Response<object>.Failure("La orden no tiene un técnico asignado");
                }

                // Only the assigned technician, or whoever can see all orders, registers the attention
                if (order.TechnicianId != _currentUser.UserId
                    && !await _currentUser.HasPermissionAsync(PermissionsCatalog.Orders.ShowAll))
                {
                    return Response<object>.Failure("La orden no está asignada a usted");
                }

                if (!OrderStatusRules.AttentionStatuses.Contains(dto.Status))
                {
                    return Response<object>.Failure($"El estado {OrderStatusRules.Name(dto.Status)} no se puede asignar desde la atención técnica");
                }

                OrderStatus previousStatus = order.Status;
                bool changesStatus = previousStatus != dto.Status;

                if (changesStatus && !OrderStatusRules.CanChange(previousStatus, dto.Status))
                {
                    return Response<object>.Failure($"No se puede pasar de {OrderStatusRules.Name(previousStatus)} a {OrderStatusRules.Name(dto.Status)}");
                }

                string? diagnosis = string.IsNullOrWhiteSpace(dto.Diagnosis) ? null : dto.Diagnosis.Trim();
                string? work = string.IsNullOrWhiteSpace(dto.WorkPerformed) ? null : dto.WorkPerformed.Trim();

                if ((dto.Status is OrderStatus.InRepair or OrderStatus.ReadyForDelivery) && diagnosis is null)
                {
                    return Response<object>.Failure("Debe registrar el diagnóstico antes de avanzar la orden");
                }

                if (dto.Status == OrderStatus.ReadyForDelivery && work is null)
                {
                    return Response<object>.Failure("Debe registrar el trabajo realizado antes de marcar la orden como lista para entrega");
                }

                order.Diagnosis = diagnosis;
                order.WorkPerformed = work;
                order.Status = dto.Status;

                string observation = string.IsNullOrWhiteSpace(dto.Observation)
                    ? (changesStatus ? $"Estado actualizado a {OrderStatusRules.Name(dto.Status)}." : "Atención técnica actualizada.")
                    : dto.Observation.Trim();

                order.FollowUps.Add(NewFollowUp(_currentUser.UserId!.Value,
                                                        DateTime.Now,
                                                        observation,
                                                        changesStatus ? previousStatus : null,
                                                        changesStatus ? dto.Status : null));

                await _context.SaveChangesAsync();

                return Response<object>.Success("Atención registrada con éxito");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        public async Task<Response<object>> DeliverAsync(DeliverOrderDTO dto)
        {
            try
            {
                ServiceOrder? order = await _context.ServiceOrders.FirstOrDefaultAsync(o => o.Id == dto.ServiceOrderId);

                if (order is null)
                {
                    return Response<object>.Failure($"No existe orden con id {dto.ServiceOrderId}");
                }

                if (order.Status != OrderStatus.ReadyForDelivery)
                {
                    return Response<object>.Failure("Solo se pueden entregar órdenes que estén listas para entrega");
                }

                DateTime now = DateTime.Now;

                order.Status = OrderStatus.Delivered;
                order.DeliveryDate = now;

                string observation = string.IsNullOrWhiteSpace(dto.Observation)
                    ? "Equipo entregado al cliente."
                    : dto.Observation.Trim();

                order.FollowUps.Add(NewFollowUp(_currentUser.UserId!.Value, now, observation,
                                                        OrderStatus.ReadyForDelivery, OrderStatus.Delivered));

                await _context.SaveChangesAsync();

                return Response<object>.Success($"Orden N° {order.Number} entregada con éxito");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        public async Task<Response<object>> CancelAsync(CancelOrderDTO dto)
        {
            try
            {
                ServiceOrder? order = await _context.ServiceOrders.FirstOrDefaultAsync(o => o.Id == dto.ServiceOrderId);

                if (order is null)
                {
                    return Response<object>.Failure($"No existe orden con id {dto.ServiceOrderId}");
                }

                if (OrderStatusRules.IsFinal(order.Status))
                {
                    return Response<object>.Failure("La orden ya fue entregada o cancelada");
                }

                DateTime now = DateTime.Now;
                OrderStatus previousStatus = order.Status;
                string reason = dto.Reason.Trim();

                order.Status = OrderStatus.Cancelled;
                order.CancellationDate = now;
                order.CancellationReason = reason;

                order.FollowUps.Add(NewFollowUp(_currentUser.UserId!.Value, now, $"Orden cancelada. Motivo: {reason}",
                                                        previousStatus, OrderStatus.Cancelled));

                await _context.SaveChangesAsync();

                return Response<object>.Success($"Orden N° {order.Number} cancelada");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        private IQueryable<ServiceOrder> QueryWithDetail()
        {
            return _context.ServiceOrders.Include(o => o.Device)
                                           .ThenInclude(e => e.Customer)
                                           .Include(o => o.Technician)
                                           .Include(o => o.ReceivedBy);
        }

        // Whoever has orders.showAll sees all of them; whoever has orders.showAssigned sees only their own
        private async Task<Response<IQueryable<ServiceOrder>>> VisibleQuery()
        {
            IQueryable<ServiceOrder> query = QueryWithDetail();

            if (await _currentUser.HasPermissionAsync(PermissionsCatalog.Orders.ShowAll))
            {
                return Response<IQueryable<ServiceOrder>>.Success(query);
            }

            if (await _currentUser.HasPermissionAsync(PermissionsCatalog.Orders.ShowAssigned))
            {
                Guid userId = _currentUser.UserId!.Value;

                return Response<IQueryable<ServiceOrder>>.Success(query.Where(o => o.TechnicianId == userId));
            }

            return Response<IQueryable<ServiceOrder>>.Failure("No tiene permiso para consultar órdenes");
        }

        private async Task<bool> CanViewAsync(ServiceOrder order)
        {
            if (await _currentUser.HasPermissionAsync(PermissionsCatalog.Orders.ShowAll))
            {
                return true;
            }

            return order.TechnicianId is not null
                && order.TechnicianId == _currentUser.UserId
                && await _currentUser.HasPermissionAsync(PermissionsCatalog.Orders.ShowAssigned);
        }

        private async Task<Response<User>> ValidateTechnicianAsync(Guid technicianId)
        {
            User? technician = await _context.Users.FirstOrDefaultAsync(u => u.Id == technicianId);

            if (technician is null || !technician.IsActive)
            {
                return Response<User>.Failure("El técnico seleccionado no existe o está inactivo");
            }

            if (!await _permissionsService.UserHasPermissionAsync(technician.Id, PermissionsCatalog.Orders.Attend))
            {
                return Response<User>.Failure("El usuario seleccionado no tiene permiso para atender órdenes");
            }

            return Response<User>.Success(technician);
        }

        private static FollowUp NewFollowUp(Guid userId, DateTime date, string observation, OrderStatus? previous, OrderStatus? newStatus)
        {
            return new FollowUp
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Date = date,
                Observation = observation,
                PreviousStatus = previous,
                NewStatus = newStatus,
            };
        }
    }
}
