using TecniControl2026.Web.Services.Abstractions;

namespace TecniControl2026.Web.Services.Implementations
{
    // Implementación de desarrollo: escribe el correo en el log en lugar de enviarlo.
    // Para producción, reemplazar por una implementación SMTP en CustomConfiguration.
    public class LogCorreoService : ICorreoService
    {
        private readonly ILogger<LogCorreoService> _logger;

        public LogCorreoService(ILogger<LogCorreoService> logger)
        {
            _logger = logger;
        }

        public Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            _logger.LogWarning("Correo para {Destinatario} | {Asunto}\n{Cuerpo}", destinatario, asunto, cuerpoHtml);

            return Task.CompletedTask;
        }
    }
}
