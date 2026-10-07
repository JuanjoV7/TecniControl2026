namespace TecniControl2026.Web.Services.Abstractions
{
    public interface ICorreoService
    {
        public Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml);
    }
}
