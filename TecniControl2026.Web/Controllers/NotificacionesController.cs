using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TecniControl2026.Web.Services;

namespace TecniControl2026.Web.Controllers
{
    public class NotificacionesController : Controller
    {
        public IActionResult Index()
        {
            
            List<NotificacionBase> historialNotificaciones = new List<NotificacionBase>
            {
                new NotificacionEmail(
                    destinatario: "cliente@correo.com",
                    mensaje: "Su equipo ha sido recibido e ingresado al sistema para diagnóstico.",
                    asunto: "TecniControl - Confirmación de Recepción de Equipo"
                ),
                new NotificacionSistema(
                    destinatario: "Orden de Servicio #101",
                    mensaje: "Asignación de técnico realizada exitosamente.",
                    usuarioResponsable: "Recepcion_Principal"
                ),
                new NotificacionEmail(
                    destinatario: "cliente@correo.com",
                    mensaje: "El diagnóstico de su equipo ha finalizado. Estado: En Reparación.",
                    asunto: "TecniControl - Actualización de Estado"
                )
            };

            List<string> registros = new List<string>();

            //Polimorfismo
            foreach (var notificacion in historialNotificaciones)
            {
                registros.Add(notificacion.EnviarNotificacion());
            }

            ViewBag.Registros = registros;
            return View();
        }
    }
}