namespace TecniControl2026.Web.Services
{
    public abstract class NotificationBase
    {
        public string Recipient { get; set; }
        public string Message { get; set; }

        public NotificationBase(string recipient, string message)
        {
            Recipient = recipient;
            Message = message;
        }

        
        public abstract string SendNotification();
    }
}