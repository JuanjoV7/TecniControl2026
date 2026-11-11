namespace TecniControl2026.Web.Services
{
    public class SystemNotification : NotificationBase
    {
        public string ResponsibleUser { get; set; }

        public SystemNotification(string recipient, string message, string responsibleUser)
            : base(recipient, message)
        {
            ResponsibleUser = responsibleUser;
        }

        public override string SendNotification()
        {
            return $"[SISTEMA INTERNO] Registrado por: {ResponsibleUser} | Notificado a: {Recipient} | Detalle: {Message}";
        }
    }
}