namespace TecniControl2026.Web.Services.Abstractions
{
    public interface IEmailService
    {
        public Task SendAsync(string recipient, string subject, string htmlBody);
    }
}
