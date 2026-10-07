namespace TecniControl2026.Web.Services
{
    public class NotificacionEmail : NotificacionBase
    {
        public string Asunto { get; set; }

        public NotificacionEmail(string destinatario, string mensaje, string asunto)
            : base(destinatario, mensaje)
        {
            Asunto = asunto;
        }

        public override string EnviarNotificacion()
        {
            return $"[EMAIL ENVIADO] Para: {Destinatario} | Asunto: {Asunto} | Mensaje: {Mensaje}";
        }
    }
}