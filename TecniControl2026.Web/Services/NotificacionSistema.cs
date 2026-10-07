namespace TecniControl2026.Web.Services
{
    public class NotificacionSistema : NotificacionBase
    {
        public string UsuarioResponsable { get; set; }

        public NotificacionSistema(string destinatario, string mensaje, string usuarioResponsable)
            : base(destinatario, mensaje)
        {
            UsuarioResponsable = usuarioResponsable;
        }

        public override string EnviarNotificacion()
        {
            return $"[SISTEMA INTERNO] Registrado por: {UsuarioResponsable} | Notificado a: {Destinatario} | Detalle: {Mensaje}";
        }
    }
}