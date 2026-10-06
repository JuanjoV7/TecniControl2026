using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TecniControl2026.Web.Core;
using TecniControl2026.Web.Core.Pagination;
using TecniControl2026.Web.Data;
using TecniControl2026.Web.Data.Abstractions;

namespace TecniControl2026.Web.Services
{
    public class CustomQueryableOperationsService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public CustomQueryableOperationsService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response<TDto>> CreateAsync<TDto, TEntity>(TDto dto) where TEntity : IId
        {
            try
            {
                TEntity entity = _mapper.Map<TEntity>(dto);

                Guid id = Guid.NewGuid();

                entity.Id = id;

                await _context.AddAsync(entity);
                await _context.SaveChangesAsync();

                return Response<TDto>.Success(dto, "Entidad creada con éxito");
            }
            catch (Exception ex)
            {
                return Response<TDto>.Failure(ex);
            }
        }

        public async Task<Response<object>> DeleteAsync<TEntity>(Guid id) where TEntity : class, IId
        {
            try
            {
                TEntity? entity = await _context.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id);

                if (entity is null)
                {
                    return Response<object>.Failure($"No existe registro con id {id}");
                }

                _context.Remove(entity);
                await _context.SaveChangesAsync();

                return Response<object>.Success("Registro eliminado con éxito");
            }
            catch (Exception ex)
            {
                return Response<object>.Failure(ex);
            }
        }

        public async Task<Response<TDto>> UpdateAsync<TDto, TEntity>(TDto dto, Guid id) where TEntity : IId
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Response<TDto>.Failure("El id no puede ser vacío");
                }

                TEntity entity = _mapper.Map<TEntity>(dto);

                entity.Id = id;

                _context.Entry(entity).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return Response<TDto>.Success(dto, "Registro actualizado con éxito");
            }
            catch (Exception ex)
            {
                return Response<TDto>.Failure(ex);
            }
        }

        public async Task<Response<TDto>> GetOneAsync<TDto, TEntity>(Guid id, IQueryable<TEntity>? query = null) where TEntity : class, IId
        {
            try
            {
                if (query is null)
                {
                    query = _context.Set<TEntity>().AsQueryable();
                }

                TEntity? entity = await query.FirstOrDefaultAsync(e => e.Id == id);

                if (entity is null)
                {
                    return Response<TDto>.Failure($"No existe registro con id {id}");
                }

                TDto dto = _mapper.Map<TDto>(entity);

                return Response<TDto>.Success(dto, "Registro obtenido con éxito");
            }
            catch (Exception ex)
            {
                return Response<TDto>.Failure(ex);
            }
        }

        public async Task<Response<PaginationResponse<TDto>>> GetPagedListAsync<TDto, TEntity>(PaginationRequest request, IQueryable<TEntity>? query = null)
        where TEntity : class
        where TDto : class
        {
            try
            {
                if (query is null)
                {
                    query = _context.Set<TEntity>().AsQueryable();
                }

                PagedList<TEntity> list = await PagedList<TEntity>.ToPagedListAsync(query, request);

                PaginationResponse<TDto> dto = new PaginationResponse<TDto>
                {
                    List = _mapper.Map<PagedList<TDto>>(list),
                    CurrentPage = list.CurrentPage,
                    TotalPages = list.TotalPages,
                    RecordsPerPage = list.RecordsPerPage,
                    TotalCount = list.TotalCount,
                    Filter = request.Filter,
                };

                return Response<PaginationResponse<TDto>>.Success(dto, "Registro obtenido con éxito");
            }
            catch (Exception ex)
            {
                return Response<PaginationResponse<TDto>>.Failure(ex);
            }
        }
    }
}
