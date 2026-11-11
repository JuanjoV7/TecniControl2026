using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    // Development implementation: writes the email to the log instead of sending it.
    // For production, replace with an SMTP implementation in CustomConfiguration.
    public class LogEmailService : IEmailService
    {
        private readonly ILogger<LogEmailService> _logger;

        public LogEmailService(ILogger<LogEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string recipient, string subject, string htmlBody)
        {
            _logger.LogWarning("Email to {Recipient} | {Subject}\n{Body}", recipient, subject, htmlBody);

            return Task.CompletedTask;
        }
    }
}
