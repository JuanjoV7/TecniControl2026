using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.DTOs.Device;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    public class DevicesService : CustomQueryableOperationsService, IDevicesService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public DevicesService(DataContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response<CreateDeviceDTO>> CreateAsync(CreateDeviceDTO dto)
        {
            Normalize(dto);

            string? error = await ValidateAsync(dto, null);

            if (error is not null)
            {
                return Response<CreateDeviceDTO>.Failure(error);
            }

            Response<CreateDeviceDTO> response = await CreateAsync<CreateDeviceDTO, Device>(dto);

            if (response.IsSuccess)
            {
                response.Message = "Equipo registrado con éxito";
            }

            return response;
        }

        public async Task<Response<object>> DeleteAsync(Guid id)
        {
            if (!await _context.Devices.AnyAsync(e => e.Id == id))
            {
                return Response<object>.Failure($"No existe equipo con id {id}");
            }

            bool hasOrders = await _context.ServiceOrders.AnyAsync(o => o.DeviceId == id);

            if (hasOrders)
            {
                return Response<object>.Failure("No se puede eliminar el equipo porque tiene órdenes de servicio asociadas");
            }

            Response<object> response = await DeleteAsync<Device>(id);

            if (response.IsSuccess)
            {
                response.Message = "Equipo eliminado con éxito";
            }

            return response;
        }

        public async Task<Response<DeviceDTO>> GetOneAsync(Guid id)
        {
            return await GetOneAsync<DeviceDTO, Device>(id, _context.Devices.Include(e => e.Customer));
        }

        public async Task<Response<PaginationResponse<DeviceDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            IQueryable<Device> query = _context.Devices.Include(e => e.Customer).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                string filter = request.Filter.Trim().ToLower();

                query = query.Where(e => e.Brand.ToLower().Contains(filter)
                                      || e.DeviceModel.ToLower().Contains(filter)
                                      || (e.SerialNumber != null && e.SerialNumber.ToLower().Contains(filter))
                                      || e.Customer.Name.ToLower().Contains(filter)
                                      || e.Customer.Document.ToLower().Contains(filter));
            }

            query = query.OrderBy(e => e.Customer.Name)
                         .ThenBy(e => e.Brand)
                         .ThenBy(e => e.DeviceModel);

            return await GetPagedListAsync<DeviceDTO, Device>(request, query);
        }

        public async Task<Response<List<DeviceDTO>>> GetByCustomerAsync(Guid customerId)
        {
            try
            {
                List<Device> devices = await _context.Devices.Include(e => e.Customer)
                                                             .Where(e => e.CustomerId == customerId)
                                                             .OrderBy(e => e.Brand)
                                                             .ThenBy(e => e.DeviceModel)
                                                             .ToListAsync();

                return Response<List<DeviceDTO>>.Success(_mapper.Map<List<DeviceDTO>>(devices));
            }
            catch (Exception ex)
            {
                return Response<List<DeviceDTO>>.Failure(ex);
            }
        }

        public async Task<Response<DeviceDTO>> UpdateAsync(UpdateDeviceDTO dto)
        {
            Normalize(dto);

            if (!await _context.Devices.AnyAsync(e => e.Id == dto.Id))
            {
                return Response<DeviceDTO>.Failure($"No existe equipo con id {dto.Id}");
            }

            string? error = await ValidateAsync(dto, dto.Id);

            if (error is not null)
            {
                return Response<DeviceDTO>.Failure(error);
            }

            Response<UpdateDeviceDTO> result = await UpdateAsync<UpdateDeviceDTO, Device>(dto, dto.Id);

            if (!result.IsSuccess)
            {
                return Response<DeviceDTO>.Failure(result.Message ?? "No fue posible actualizar el equipo", result.Errors);
            }

            Response<DeviceDTO> response = await GetOneAsync(dto.Id);
            response.Message = "Equipo actualizado con éxito";

            return response;
        }

        private static void Normalize(CreateDeviceDTO dto)
        {
            dto.Brand = dto.Brand.Trim();
            dto.DeviceModel = dto.DeviceModel.Trim();
            dto.SerialNumber = string.IsNullOrWhiteSpace(dto.SerialNumber) ? null : dto.SerialNumber.Trim();
        }

        private async Task<string?> ValidateAsync(CreateDeviceDTO dto, Guid? deviceId)
        {
            Customer? customer = await _context.Customers.AsNoTracking()
                                                      .FirstOrDefaultAsync(c => c.Id == dto.CustomerId);

            if (customer is null)
            {
                return "El cliente seleccionado no existe";
            }

            if (!customer.IsActive)
            {
                return "El cliente seleccionado está inactivo";
            }

            if (dto.SerialNumber is not null
                && await _context.Devices.AnyAsync(e => e.SerialNumber == dto.SerialNumber && e.Id != deviceId))
            {
                return "Ya existe un equipo con ese serial";
            }

            return null;
        }
    }
}
