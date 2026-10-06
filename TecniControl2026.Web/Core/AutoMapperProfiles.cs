using AutoMapper;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.DTOs.Cliente;

namespace TecniControl2026.Web.Core
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Cliente, ClienteDTO>().ReverseMap();

            CreateMap<Cliente, CreateClienteDTO>().ReverseMap();

            CreateMap<Cliente, UpdateClienteDTO>().ReverseMap();
        }
    }
}
