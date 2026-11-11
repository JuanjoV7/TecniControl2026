using AutoMapper;
using TecniControl2026.Web.Data.Entities;
using TecniControl2026.Web.DTOs.Customer;
using TecniControl2026.Web.DTOs.Device;
using TecniControl2026.Web.DTOs.ServiceOrder;
using TecniControl2026.Web.DTOs.Role;
using TecniControl2026.Web.DTOs.FollowUp;
using TecniControl2026.Web.DTOs.User;

namespace TecniControl2026.Web.Core
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Customers
            CreateMap<Customer, CustomerDTO>().ReverseMap();

            CreateMap<Customer, CreateCustomerDTO>().ReverseMap();

            CreateMap<Customer, UpdateCustomerDTO>().ReverseMap();

            // Devices (CustomerName and CustomerDocument are flattened from Customer)
            CreateMap<Device, DeviceDTO>();

            CreateMap<CreateDeviceDTO, Device>();

            CreateMap<UpdateDeviceDTO, Device>();

            CreateMap<Device, UpdateDeviceDTO>();

            // Service orders
            CreateMap<ServiceOrder, ServiceOrderDTO>()
                .ForMember(d => d.DeviceDescription, o => o.MapFrom(s => s.Device.Type + " " + s.Device.Brand + " " + s.Device.DeviceModel))
                .ForMember(d => d.CustomerId, o => o.MapFrom(s => s.Device.CustomerId))
                .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Device.Customer.Name))
                .ForMember(d => d.TechnicianName, o => o.MapFrom(s => s.Technician != null ? s.Technician.FirstName + " " + s.Technician.LastName : null));

            CreateMap<ServiceOrder, ServiceOrderDetailDTO>()
                .IncludeBase<ServiceOrder, ServiceOrderDTO>()
                .ForMember(d => d.CustomerPhone, o => o.MapFrom(s => s.Device.Customer.Phone))
                .ForMember(d => d.ReceivedByName, o => o.MapFrom(s => s.ReceivedBy.FirstName + " " + s.ReceivedBy.LastName));

            // Follow-ups
            CreateMap<FollowUp, FollowUpDTO>()
                .ForMember(d => d.UserFullName, o => o.MapFrom(s => s.User.FirstName + " " + s.User.LastName));

            // Users (RoleName is flattened from Role)
            CreateMap<User, UserDTO>();

            CreateMap<User, UpdateUserDTO>();

            // Roles and permissions
            CreateMap<Permission, PermissionDTO>();
        }
    }
}
