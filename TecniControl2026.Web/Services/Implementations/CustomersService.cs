using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.DTOs.Customer;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    public class CustomersService : CustomQueryableOperationsService, ICustomersService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public CustomersService(DataContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response<CreateCustomerDTO>> CreateAsync(CreateCustomerDTO dto)
        {
            dto.Document = dto.Document.Trim();

            if (await _context.Customers.AnyAsync(c => c.Document == dto.Document))
            {
                return Response<CreateCustomerDTO>.Failure("Ya existe un cliente con ese documento");
            }

            Response<CreateCustomerDTO> response = await CreateAsync<CreateCustomerDTO, Customer>(dto);

            if (response.IsSuccess)
            {
                response.Message = "Cliente registrado con éxito";
            }

            return response;
        }

        public async Task<Response<CustomerDTO>> GetOneAsync(Guid id)
        {
            return await GetOneAsync<CustomerDTO, Customer>(id);
        }

        public async Task<Response<List<CustomerDTO>>> GetActiveAsync()
        {
            try
            {
                List<Customer> customers = await _context.Customers.AsNoTracking()
                                                               .Where(c => c.IsActive)
                                                               .OrderBy(c => c.Name)
                                                               .ToListAsync();

                return Response<List<CustomerDTO>>.Success(_mapper.Map<List<CustomerDTO>>(customers));
            }
            catch (Exception ex)
            {
                return Response<List<CustomerDTO>>.Failure(ex);
            }
        }

        public async Task<Response<PaginationResponse<CustomerDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            IQueryable<Customer> query = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                // SELECT * FROM Customers WHERE LOWER(Name) LIKE '%filter%' OR ...
                string filter = request.Filter.Trim().ToLower();

                query = query.Where(c => c.Name.ToLower().Contains(filter)
                                      || c.Document.ToLower().Contains(filter)
                                      || (c.Phone != null && c.Phone.ToLower().Contains(filter))
                                      || (c.Email != null && c.Email.ToLower().Contains(filter)));
            }

            query = query.OrderBy(c => c.Name);

            return await GetPagedListAsync<CustomerDTO, Customer>(request, query);
        }

        public async Task<Response<CustomerDTO>> UpdateAsync(UpdateCustomerDTO dto)
        {
            dto.Document = dto.Document.Trim();

            if (await _context.Customers.AnyAsync(c => c.Document == dto.Document && c.Id != dto.Id))
            {
                return Response<CustomerDTO>.Failure("Ya existe otro cliente con ese documento");
            }

            Response<UpdateCustomerDTO> result = await UpdateAsync<UpdateCustomerDTO, Customer>(dto, dto.Id);

            if (!result.IsSuccess)
            {
                return Response<CustomerDTO>.Failure(result.Message ?? "No fue posible actualizar el cliente", result.Errors);
            }

            CustomerDTO dtoResponse = new CustomerDTO
            {
                Id = dto.Id,
                Document = dto.Document,
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                IsActive = dto.IsActive
            };

            return Response<CustomerDTO>.Success(dtoResponse, "Cliente actualizado con éxito");
        }

        public async Task<Response<object>> ToggleAsync(ToggleCustomerStatusDTO dto)
        {
            try
            {
                Customer? customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == dto.CustomerId);

                if (customer is null)
                {
                    return Response<object>.Failure($"No existe cliente con id: {dto.CustomerId}");
                }

                customer.IsActive = dto.IsActive;
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();

                return Response<object>.Success(dto.IsActive ? "Cliente activado con éxito" : "Cliente desactivado con éxito");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }
    }
}
