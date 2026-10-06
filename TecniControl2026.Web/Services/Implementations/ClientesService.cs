using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.DTOs.Cliente;
using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    public class ClientesService : CustomQueryableOperationsService, IClientesService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public ClientesService(DataContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response<CreateClienteDTO>> CreateAsync(CreateClienteDTO dto)
        {
            dto.Documento = dto.Documento.Trim();

            if (await _context.Clientes.AnyAsync(c => c.Documento == dto.Documento))
            {
                return Response<CreateClienteDTO>.Failure("Ya existe un cliente con ese documento");
            }

            Response<CreateClienteDTO> response = await CreateAsync<CreateClienteDTO, Cliente>(dto);

            if (response.IsSuccess)
            {
                response.Message = "Cliente registrado con éxito";
            }

            return response;
        }

        public async Task<Response<ClienteDTO>> GetOneAsync(Guid id)
        {
            return await GetOneAsync<ClienteDTO, Cliente>(id);
        }

        public async Task<Response<PaginationResponse<ClienteDTO>>> GetPaginationAsync(PaginationRequest request)
        {
            IQueryable<Cliente> query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                // SELECT * FROM Clientes WHERE LOWER(Nombre) LIKE '%filtro%' OR ...
                string filter = request.Filter.Trim().ToLower();

                query = query.Where(c => c.Nombre.ToLower().Contains(filter)
                                      || c.Documento.ToLower().Contains(filter)
                                      || (c.Telefono != null && c.Telefono.ToLower().Contains(filter))
                                      || (c.Email != null && c.Email.ToLower().Contains(filter)));
            }

            query = query.OrderBy(c => c.Nombre);

            return await GetPagedListAsync<ClienteDTO, Cliente>(request, query);
        }

        public async Task<Response<ClienteDTO>> UpdateAsync(UpdateClienteDTO dto)
        {
            dto.Documento = dto.Documento.Trim();

            if (await _context.Clientes.AnyAsync(c => c.Documento == dto.Documento && c.Id != dto.Id))
            {
                return Response<ClienteDTO>.Failure("Ya existe otro cliente con ese documento");
            }

            Response<UpdateClienteDTO> result = await UpdateAsync<UpdateClienteDTO, Cliente>(dto, dto.Id);

            if (!result.IsSuccess)
            {
                return Response<ClienteDTO>.Failure(result.Message ?? "No fue posible actualizar el cliente", result.Errors);
            }

            ClienteDTO dtoResponse = new ClienteDTO
            {
                Id = dto.Id,
                Documento = dto.Documento,
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Activo = dto.Activo
            };

            return Response<ClienteDTO>.Success(dtoResponse, "Cliente actualizado con éxito");
        }

        public async Task<Response<object>> ToggleAsync(ToggleClienteStatusDTO dto)
        {
            try
            {
                Cliente? cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == dto.ClienteId);

                if (cliente is null)
                {
                    return Response<object>.Failure($"No existe cliente con id: {dto.ClienteId}");
                }

                cliente.Activo = dto.Activo;
                _context.Clientes.Update(cliente);
                await _context.SaveChangesAsync();

                return Response<object>.Success(dto.Activo ? "Cliente activado con éxito" : "Cliente desactivado con éxito");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }
    }
}
