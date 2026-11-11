namespace TecniControl2026.Web.Services
{
    public class EmailNotification : NotificationBase
    {
        public string Subject { get; set; }

        public EmailNotification(string recipient, string message, string subject)
            : base(recipient, message)
        {
            Subject = subject;
        }

        public override string SendNotification()
        {
            return $"[EMAIL ENVIADO] Para: {Recipient} | Asunto: {Subject} | Mensaje: {Message}";
        }
    }
}