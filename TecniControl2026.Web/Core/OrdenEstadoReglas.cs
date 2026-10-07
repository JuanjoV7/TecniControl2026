using TecniControl2026.Web.Data.Enums;

namespace TecniControl2026.Web.Core
{
    // Flujo de una orden: Recibida → EnDiagnostico → EnReparacion → ListaParaEntrega → Entregada.
    // Se puede cancelar en cualquier momento antes de la entrega.
    public static class OrdenEstadoReglas
    {
        private static readonly Dictionary<EstadoOrden, EstadoOrden[]> _transiciones = new()
        {
            [EstadoOrden.Recibida] = new[] { EstadoOrden.EnDiagnostico, EstadoOrden.Cancelada },
            [EstadoOrden.EnDiagnostico] = new[] { EstadoOrden.EnReparacion, EstadoOrden.ListaParaEntrega, EstadoOrden.Cancelada },
            [EstadoOrden.EnReparacion] = new[] { EstadoOrden.EnDiagnostico, EstadoOrden.ListaParaEntrega, EstadoOrden.Cancelada },
            [EstadoOrden.ListaParaEntrega] = new[] { EstadoOrden.EnReparacion, EstadoOrden.Entregada, EstadoOrden.Cancelada },
            [EstadoOrden.Entregada] = Array.Empty<EstadoOrden>(),
            [EstadoOrden.Cancelada] = Array.Empty<EstadoOrden>(),
        };

        // Estados que el técnico puede asignar al registrar su atención
        public static readonly EstadoOrden[] EstadosDeAtencion =
        {
            EstadoOrden.EnDiagnostico,
            EstadoOrden.EnReparacion,
            EstadoOrden.ListaParaEntrega,
        };

        public static bool EsFinal(EstadoOrden estado)
        {
            return estado == EstadoOrden.Entregada || estado == EstadoOrden.Cancelada;
        }

        public static bool PuedeCambiar(EstadoOrden actual, EstadoOrden nuevo)
        {
            return _transiciones[actual].Contains(nuevo);
        }

        public static IEnumerable<EstadoOrden> SiguientesEstados(EstadoOrden actual)
        {
            return _transiciones[actual];
        }

        public static string Nombre(EstadoOrden estado)
        {
            return estado switch
            {
                EstadoOrden.Recibida => "Recibida",
                EstadoOrden.EnDiagnostico => "En diagnóstico",
                EstadoOrden.EnReparacion => "En reparación",
                EstadoOrden.ListaParaEntrega => "Lista para entrega",
                EstadoOrden.Entregada => "Entregada",
                EstadoOrden.Cancelada => "Cancelada",
                _ => estado.ToString()
            };
        }
    }
}
