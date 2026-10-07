namespace TecniControl2026.Web.Services
{
    public abstract class NotificacionBase
    {
        public string Destinatario { get; set; }
        public string Mensaje { get; set; }

        public NotificacionBase(string destinatario, string mensaje)
        {
            Destinatario = destinatario;
            Mensaje = mensaje;
        }

        
        public abstract string EnviarNotificacion();
    }
}