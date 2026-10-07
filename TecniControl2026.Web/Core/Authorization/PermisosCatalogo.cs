namespace TecniControl2026.Web.Core.Authorization
{
    // Catálogo de acciones que el sistema puede autorizar.
    // Al iniciar la aplicación se sincroniza con la tabla Permisos (ver SeedDb).
    public static class PermisosCatalogo
    {
        public static class Clientes
        {
            public const string Ver = "Clientes.Ver";
            public const string Crear = "Clientes.Crear";
            public const string Editar = "Clientes.Editar";
        }

        public static class Equipos
        {
            public const string Ver = "Equipos.Ver";
            public const string Crear = "Equipos.Crear";
            public const string Editar = "Equipos.Editar";
        }

        public static class Ordenes
        {
            public const string VerTodas = "Ordenes.VerTodas";
            public const string VerAsignadas = "Ordenes.VerAsignadas";
            public const string Crear = "Ordenes.Crear";
            public const string AsignarTecnico = "Ordenes.AsignarTecnico";
            public const string Atender = "Ordenes.Atender";
            public const string Entregar = "Ordenes.Entregar";
            public const string Cancelar = "Ordenes.Cancelar";
        }

        public static class Seguimientos
        {
            public const string Crear = "Seguimientos.Crear";
        }

        public static class Usuarios
        {
            public const string Ver = "Usuarios.Ver";
            public const string Crear = "Usuarios.Crear";
            public const string Editar = "Usuarios.Editar";
        }

        public static class Roles
        {
            public const string Ver = "Roles.Ver";
            public const string Crear = "Roles.Crear";
            public const string Editar = "Roles.Editar";
            public const string Eliminar = "Roles.Eliminar";
        }

        public record PermisoDefinicion(string Codigo, string Modulo, string Descripcion);

        public static readonly IReadOnlyList<PermisoDefinicion> Todos = new List<PermisoDefinicion>
        {
            new(Clientes.Ver, "Clientes", "Consultar clientes"),
            new(Clientes.Crear, "Clientes", "Registrar clientes"),
            new(Clientes.Editar, "Clientes", "Actualizar, activar y desactivar clientes"),

            new(Equipos.Ver, "Equipos", "Consultar equipos"),
            new(Equipos.Crear, "Equipos", "Registrar equipos"),
            new(Equipos.Editar, "Equipos", "Actualizar equipos"),

            new(Ordenes.VerTodas, "Órdenes de servicio", "Consultar todas las órdenes"),
            new(Ordenes.VerAsignadas, "Órdenes de servicio", "Consultar las órdenes asignadas al usuario"),
            new(Ordenes.Crear, "Órdenes de servicio", "Registrar la recepción de equipos"),
            new(Ordenes.AsignarTecnico, "Órdenes de servicio", "Asignar o cambiar el técnico de una orden"),
            new(Ordenes.Atender, "Órdenes de servicio", "Registrar diagnóstico, trabajo realizado y avance"),
            new(Ordenes.Entregar, "Órdenes de servicio", "Registrar la entrega al cliente"),
            new(Ordenes.Cancelar, "Órdenes de servicio", "Cancelar órdenes"),

            new(Seguimientos.Crear, "Seguimiento", "Agregar observaciones a una orden"),

            new(Usuarios.Ver, "Usuarios", "Consultar usuarios"),
            new(Usuarios.Crear, "Usuarios", "Registrar usuarios"),
            new(Usuarios.Editar, "Usuarios", "Actualizar, activar y desactivar usuarios"),

            new(Roles.Ver, "Roles y permisos", "Consultar roles y permisos"),
            new(Roles.Crear, "Roles y permisos", "Crear roles"),
            new(Roles.Editar, "Roles y permisos", "Actualizar roles y asignar permisos"),
            new(Roles.Eliminar, "Roles y permisos", "Eliminar roles"),
        };
    }

    public static class RolesBase
    {
        public const string Administrador = "Administrador";
        public const string Recepcionista = "Recepcionista";
        public const string Tecnico = "Técnico";

        public static readonly IReadOnlyDictionary<string, (string Descripcion, string[] Permisos)> Definiciones =
            new Dictionary<string, (string, string[])>
            {
                [Administrador] = (
                    "Gestiona usuarios, roles y permisos, y puede realizar las operaciones del taller",
                    PermisosCatalogo.Todos.Select(p => p.Codigo).ToArray()),

                [Recepcionista] = (
                    "Registra clientes, equipos y órdenes; asigna técnicos y registra entregas o cancelaciones",
                    new[]
                    {
                        PermisosCatalogo.Clientes.Ver, PermisosCatalogo.Clientes.Crear, PermisosCatalogo.Clientes.Editar,
                        PermisosCatalogo.Equipos.Ver, PermisosCatalogo.Equipos.Crear, PermisosCatalogo.Equipos.Editar,
                        PermisosCatalogo.Ordenes.VerTodas, PermisosCatalogo.Ordenes.Crear, PermisosCatalogo.Ordenes.AsignarTecnico,
                        PermisosCatalogo.Ordenes.Entregar, PermisosCatalogo.Ordenes.Cancelar,
                        PermisosCatalogo.Seguimientos.Crear,
                    }),

                [Tecnico] = (
                    "Consulta las órdenes asignadas y registra diagnósticos, trabajos realizados y avances",
                    new[]
                    {
                        PermisosCatalogo.Equipos.Ver,
                        PermisosCatalogo.Ordenes.VerAsignadas, PermisosCatalogo.Ordenes.Atender,
                        PermisosCatalogo.Seguimientos.Crear,
                    }),
            };
    }
}
