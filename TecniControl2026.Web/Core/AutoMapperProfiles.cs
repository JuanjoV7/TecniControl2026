using AutoMapper;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.DTOs.Cliente;
using TecniControl2026.Web.DTOs.Equipo;
using TecniControl2026.Web.DTOs.OrdenServicio;
using TecniControl2026.Web.DTOs.Rol;
using TecniControl2026.Web.DTOs.Seguimiento;
using TecniControl2026.Web.DTOs.Usuario;

namespace TecniControl2026.Web.Core
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Clientes
            CreateMap<Cliente, ClienteDTO>().ReverseMap();

            CreateMap<Cliente, CreateClienteDTO>().ReverseMap();

            CreateMap<Cliente, UpdateClienteDTO>().ReverseMap();

            // Equipos (ClienteNombre y ClienteDocumento se aplanan desde Cliente)
            CreateMap<Equipo, EquipoDTO>();

            CreateMap<CreateEquipoDTO, Equipo>();

            CreateMap<UpdateEquipoDTO, Equipo>();

            CreateMap<Equipo, UpdateEquipoDTO>();

            // Órdenes de servicio
            CreateMap<OrdenServicio, OrdenServicioDTO>()
                .ForMember(d => d.EquipoDescripcion, o => o.MapFrom(s => s.Equipo.Tipo + " " + s.Equipo.Marca + " " + s.Equipo.Modelo))
                .ForMember(d => d.ClienteId, o => o.MapFrom(s => s.Equipo.ClienteId))
                .ForMember(d => d.ClienteNombre, o => o.MapFrom(s => s.Equipo.Cliente.Nombre))
                .ForMember(d => d.TecnicoNombre, o => o.MapFrom(s => s.Tecnico != null ? s.Tecnico.Nombres + " " + s.Tecnico.Apellidos : null));

            CreateMap<OrdenServicio, OrdenServicioDetalleDTO>()
                .IncludeBase<OrdenServicio, OrdenServicioDTO>()
                .ForMember(d => d.ClienteTelefono, o => o.MapFrom(s => s.Equipo.Cliente.Telefono))
                .ForMember(d => d.RecibidaPorNombre, o => o.MapFrom(s => s.RecibidaPor.Nombres + " " + s.RecibidaPor.Apellidos));

            // Seguimientos
            CreateMap<Seguimiento, SeguimientoDTO>()
                .ForMember(d => d.UsuarioNombre, o => o.MapFrom(s => s.Usuario.Nombres + " " + s.Usuario.Apellidos));

            // Usuarios (RolNombre se aplana desde Rol)
            CreateMap<Usuario, UsuarioDTO>();

            CreateMap<Usuario, UpdateUsuarioDTO>();

            // Roles y permisos
            CreateMap<Permiso, PermisoDTO>();
        }
    }
}
