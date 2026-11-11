namespace TecniControl2026.Web.Core.Authorization
{
    // Catalog of actions the system can authorize.
    // On startup it is synchronized with the Permissions table (see SeedDb).
    public static class PermissionsCatalog
    {
        public static class Customers
        {
            public const string Show = "customers.show";
            public const string Create = "customers.create";
            public const string Update = "customers.update";
        }

        public static class Devices
        {
            public const string Show = "devices.show";
            public const string Create = "devices.create";
            public const string Update = "devices.update";
        }

        public static class Orders
        {
            public const string ShowAll = "orders.showAll";
            public const string ShowAssigned = "orders.showAssigned";
            public const string Create = "orders.create";
            public const string AssignTechnician = "orders.assignTechnician";
            public const string Attend = "orders.attend";
            public const string Deliver = "orders.deliver";
            public const string Cancel = "orders.cancel";
        }

        public static class FollowUps
        {
            public const string Create = "followups.create";
        }

        public static class Users
        {
            public const string Show = "users.show";
            public const string Create = "users.create";
            public const string Update = "users.update";
        }

        public static class Roles
        {
            public const string Show = "roles.show";
            public const string Create = "roles.create";
            public const string Update = "roles.update";
            public const string Delete = "roles.delete";
        }

        public record PermissionDefinition(string Code, string Module, string Description);

        public static readonly IReadOnlyList<PermissionDefinition> All = new List<PermissionDefinition>
        {
            new(Customers.Show, "Clientes", "Consultar clientes"),
            new(Customers.Create, "Clientes", "Registrar clientes"),
            new(Customers.Update, "Clientes", "Actualizar, activar y desactivar clientes"),

            new(Devices.Show, "Equipos", "Consultar equipos"),
            new(Devices.Create, "Equipos", "Registrar equipos"),
            new(Devices.Update, "Equipos", "Actualizar equipos"),

            new(Orders.ShowAll, "Órdenes de servicio", "Consultar todas las órdenes"),
            new(Orders.ShowAssigned, "Órdenes de servicio", "Consultar las órdenes asignadas al usuario"),
            new(Orders.Create, "Órdenes de servicio", "Registrar la recepción de equipos"),
            new(Orders.AssignTechnician, "Órdenes de servicio", "Asignar o cambiar el técnico de una orden"),
            new(Orders.Attend, "Órdenes de servicio", "Registrar diagnóstico, trabajo realizado y avance"),
            new(Orders.Deliver, "Órdenes de servicio", "Registrar la entrega al cliente"),
            new(Orders.Cancel, "Órdenes de servicio", "Cancelar órdenes"),

            new(FollowUps.Create, "Seguimiento", "Agregar observaciones a una orden"),

            new(Users.Show, "Usuarios", "Consultar usuarios"),
            new(Users.Create, "Usuarios", "Registrar usuarios"),
            new(Users.Update, "Usuarios", "Actualizar, activar y desactivar usuarios"),

            new(Roles.Show, "Roles y permisos", "Consultar roles y permisos"),
            new(Roles.Create, "Roles y permisos", "Crear roles"),
            new(Roles.Update, "Roles y permisos", "Actualizar roles y asignar permisos"),
            new(Roles.Delete, "Roles y permisos", "Eliminar roles"),
        };
    }

    public static class BaseRoles
    {
        public const string Administrator = "Administrador";
        public const string Receptionist = "Recepcionista";
        public const string Technician = "Técnico";

        public static readonly IReadOnlyDictionary<string, (string Description, string[] Permissions)> Definitions =
            new Dictionary<string, (string, string[])>
            {
                [Administrator] = (
                    "Gestiona usuarios, roles y permisos, y puede realizar las operaciones del taller",
                    PermissionsCatalog.All.Select(p => p.Code).ToArray()),

                [Receptionist] = (
                    "Registra clientes, equipos y órdenes; asigna técnicos y registra entregas o cancelaciones",
                    new[]
                    {
                        PermissionsCatalog.Customers.Show, PermissionsCatalog.Customers.Create, PermissionsCatalog.Customers.Update,
                        PermissionsCatalog.Devices.Show, PermissionsCatalog.Devices.Create, PermissionsCatalog.Devices.Update,
                        PermissionsCatalog.Orders.ShowAll, PermissionsCatalog.Orders.Create, PermissionsCatalog.Orders.AssignTechnician,
                        PermissionsCatalog.Orders.Deliver, PermissionsCatalog.Orders.Cancel,
                        PermissionsCatalog.FollowUps.Create,
                    }),

                [Technician] = (
                    "Consulta las órdenes asignadas y registra diagnósticos, trabajos realizados y avances",
                    new[]
                    {
                        PermissionsCatalog.Devices.Show,
                        PermissionsCatalog.Orders.ShowAssigned, PermissionsCatalog.Orders.Attend,
                        PermissionsCatalog.FollowUps.Create,
                    }),
            };
    }
}
