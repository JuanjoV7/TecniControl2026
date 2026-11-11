using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TecniControl2026.Web.Services;

namespace TecniControl2026.Web.Controllers
{
    public class NotificationsController : Controller
    {
        public IActionResult Index()
        {
            
            List<NotificationBase> notificationHistory = new List<NotificationBase>
            {
                new EmailNotification(
                    recipient: "cliente@correo.com",
                    message: "Su equipo ha sido recibido e ingresado al sistema para diagnóstico.",
                    subject: "TecniControl - Confirmación de Recepción de Equipo"
                ),
                new SystemNotification(
                    recipient: "Orden de Servicio #101",
                    message: "Asignación de técnico realizada exitosamente.",
                    responsibleUser: "Recepcion_Principal"
                ),
                new EmailNotification(
                    recipient: "cliente@correo.com",
                    message: "El diagnóstico de su equipo ha finalizado. Estado: En Reparación.",
                    subject: "TecniControl - Actualización de Estado"
                )
            };

            List<string> records = new List<string>();

            // Polymorphism
            foreach (var notification in notificationHistory)
            {
                records.Add(notification.SendNotification());
            }

            ViewBag.Records = records;
            return View();
        }
    }
}